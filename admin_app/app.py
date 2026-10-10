import os
import json
import io
import psycopg2
import psycopg2.extras
import redis
import boto3
from botocore.client import Config
import pandas as pd
import streamlit as st

st.set_page_config(
    page_title="RestoCore Unified Admin",
    page_icon="Console",
    layout="wide",
    initial_sidebar_state="expanded",
)

# ---------------------------------------------------------
# Sidebar: Connection Parameters & Status
# ---------------------------------------------------------
st.sidebar.title("Unified Admin Configuration")

with st.sidebar.expander("PostgreSQL Settings", expanded=False):
    pg_host = st.text_input("PG Host", value=os.getenv("POSTGRES_HOST", "postgres"))
    pg_port = st.number_input("PG Port", value=int(os.getenv("POSTGRES_PORT", "5432")), step=1)
    pg_db = st.text_input("PG Database", value=os.getenv("POSTGRES_DB", "restocore_dev"))
    pg_user = st.text_input("PG User", value=os.getenv("POSTGRES_USER", "postgres"))
    pg_password = st.text_input("PG Password", value=os.getenv("POSTGRES_PASSWORD", "postgres_dev_password"), type="password")

with st.sidebar.expander("Redis / Pyrefly Settings", expanded=False):
    redis_host = st.text_input("Redis Host", value=os.getenv("REDIS_HOST", "redis"))
    redis_port = st.number_input("Redis Port", value=int(os.getenv("REDIS_PORT", "6379")), step=1)
    redis_db = st.number_input("Redis DB Index", value=0, min_value=0, max_value=15, step=1)

with st.sidebar.expander("SeaweedFS (S3) Settings", expanded=False):
    s3_endpoint = st.text_input("S3 Endpoint", value=os.getenv("S3_ENDPOINT_URL", "http://seaweedfs:8333"))
    s3_access_key = st.text_input("Access Key", value=os.getenv("AWS_ACCESS_KEY_ID", "any_key"))
    s3_secret_key = st.text_input("Secret Key", value=os.getenv("AWS_SECRET_ACCESS_KEY", "any_secret"), type="password")
    s3_region = st.text_input("Region", value=os.getenv("AWS_DEFAULT_REGION", "us-east-1"))

st.sidebar.markdown("---")
st.sidebar.markdown("**RestoCore Unified Stack**")
st.sidebar.caption("Modular control plane for PostgreSQL, Redis/Pyrefly and SeaweedFS (S3).")


# ---------------------------------------------------------
# Connection Helpers
# ---------------------------------------------------------
def get_pg_connection():
    return psycopg2.connect(
        host=pg_host,
        port=pg_port,
        dbname=pg_db,
        user=pg_user,
        password=pg_password,
        connect_timeout=5,
    )


def get_redis_client():
    return redis.Redis(
        host=redis_host,
        port=redis_port,
        db=redis_db,
        decode_responses=True,
        socket_timeout=5,
    )


def get_s3_client():
    return boto3.client(
        "s3",
        endpoint_url=s3_endpoint,
        aws_access_key_id=s3_access_key,
        aws_secret_access_key=s3_secret_key,
        region_name=s3_region,
        config=Config(signature_version="s3v4"),
    )


# ---------------------------------------------------------
# Main Interface: Tabs
# ---------------------------------------------------------
st.title("RestoCore Unified Admin Console")
st.caption("Live management of relational data, distributed cache and object storage.")

tab_pg, tab_redis, tab_s3 = st.tabs(["PostgreSQL", "Redis (Pyrefly / Cache)", "SeaweedFS (S3)"])


# =========================================================
# TAB 1: PostgreSQL
# =========================================================
with tab_pg:
    st.subheader("PostgreSQL Database Explorer")
    try:
        with get_pg_connection() as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT table_name 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    ORDER BY table_name;
                    """
                )
                tables = [row[0] for row in cur.fetchall()]

            if not tables:
                st.warning("No tables found in public schema.")
            else:
                col_table, col_limit, col_offset = st.columns([3, 1, 1])
                with col_table:
                    selected_table = st.selectbox("Select Table", options=tables, index=0)
                with col_limit:
                    limit = st.number_input("Limit Rows", min_value=10, max_value=1000, value=100, step=10)
                with col_offset:
                    offset = st.number_input("Offset", min_value=0, max_value=100000, value=0, step=10)

                if selected_table:
                    with conn.cursor() as cur:
                        cur.execute(f"SELECT COUNT(*) FROM \"{selected_table}\";")
                        total_count = cur.fetchone()[0]

                    st.markdown(f"**Table:** `{selected_table}` | **Total Rows:** `{total_count}`")

                    # Load Columns and Rows
                    query = f"SELECT * FROM \"{selected_table}\" LIMIT {limit} OFFSET {offset};"
                    df = pd.read_sql_query(query, conn)

                    st.dataframe(df, use_container_width=True)

                    with st.expander("Table Schema Columns", expanded=False):
                        with conn.cursor() as cur:
                            cur.execute(
                                """
                                SELECT column_name, data_type, is_nullable, column_default
                                FROM information_schema.columns
                                WHERE table_schema = 'public' AND table_name = %s
                                ORDER BY ordinal_position;
                                """,
                                (selected_table,),
                            )
                            cols = cur.fetchall()
                            cols_df = pd.DataFrame(
                                cols, columns=["Column Name", "Data Type", "Nullable", "Default Value"]
                            )
                            st.dataframe(cols_df, use_container_width=True)

            st.markdown("---")
            st.markdown("### Execute Custom SQL Query")
            custom_sql = st.text_area("SQL Statement (SELECT only recommended for read-only inspections):", value="", height=100)
            if st.button("Run SQL Query", key="btn_run_sql"):
                if custom_sql.strip():
                    try:
                        with conn.cursor() as cur:
                            cur.execute(custom_sql)
                            if cur.description:
                                result_df = pd.DataFrame(cur.fetchall(), columns=[desc[0] for desc in cur.description])
                                st.success(f"Query executed successfully ({len(result_df)} rows).")
                                st.dataframe(result_df, use_container_width=True)
                            else:
                                conn.commit()
                                st.success("Command executed successfully.")
                    except Exception as sql_err:
                        st.error(f"SQL Error: {sql_err}")
                else:
                    st.info("Enter an SQL statement to run.")

    except Exception as e:
        st.error(f"PostgreSQL connection error: {e}")


# =========================================================
# TAB 2: Redis / Pyrefly
# =========================================================
with tab_redis:
    st.subheader("Redis / Pyrefly Cache Inspector")
    try:
        r = get_redis_client()
        r.ping()

        col_filter, col_scan_btn = st.columns([3, 1])
        with col_filter:
            pattern = st.text_input("Key Search Pattern", value="*")
        with col_scan_btn:
            st.write("")
            st.write("")
            refresh_redis = st.button("Refresh Keys", key="btn_refresh_redis")

        # Scan keys
        keys = []
        cursor = 0
        while True:
            cursor, batch = r.scan(cursor=cursor, match=pattern, count=100)
            keys.extend(batch)
            if cursor == 0 or len(keys) >= 500:
                break

        keys = sorted(keys)

        st.markdown(f"**Found Keys:** `{len(keys)}` (showing up to 500)")

        if keys:
            selected_key = st.selectbox("Select Key", options=keys)

            if selected_key:
                key_type = r.type(selected_key)
                ttl = r.ttl(selected_key)

                c1, c2, c3 = st.columns(3)
                c1.metric("Key Name", selected_key)
                c2.metric("Data Type", key_type)
                c3.metric("TTL (seconds)", "Persistent (-1)" if ttl == -1 else (f"{ttl}s" if ttl >= 0 else "Expired/None"))

                st.markdown("#### Key Value Content")

                if key_type == "string":
                    val = r.get(selected_key)
                    try:
                        # Attempt to parse and pretty-print JSON if formatted
                        parsed_json = json.loads(val)
                        st.json(parsed_json)
                    except Exception:
                        st.code(val, language="text")

                elif key_type == "hash":
                    hash_val = r.hgetall(selected_key)
                    st.dataframe(pd.DataFrame(list(hash_val.items()), columns=["Field", "Value"]), use_container_width=True)

                elif key_type == "list":
                    list_val = r.lrange(selected_key, 0, 100)
                    st.write(list_val)

                elif key_type == "set":
                    set_val = list(r.smembers(selected_key))
                    st.write(set_val)

                elif key_type == "zset":
                    zset_val = r.zrange(selected_key, 0, 100, withscores=True)
                    st.dataframe(pd.DataFrame(zset_val, columns=["Member", "Score"]), use_container_width=True)

                else:
                    st.write(f"Unsupported key type rendering: {key_type}")

                col_del, _ = st.columns([1, 4])
                with col_del:
                    if st.button("Delete Key", key=f"del_{selected_key}"):
                        r.delete(selected_key)
                        st.success(f"Key `{selected_key}` deleted.")
                        st.rerun()
        else:
            st.info(f"No keys matching pattern '{pattern}' found in DB {redis_db}.")

        # Server Info
        with st.expander("Redis Server Info & Stats", expanded=False):
            info = r.info()
            st.json({
                "redis_version": info.get("redis_version"),
                "connected_clients": info.get("connected_clients"),
                "used_memory_human": info.get("used_memory_human"),
                "uptime_in_days": info.get("uptime_in_days"),
                "total_keys": info.get("total_keys", "N/A"),
            })

    except Exception as e:
        st.error(f"Redis / Pyrefly connection error: {e}")


# =========================================================
# TAB 3: SeaweedFS (S3)
# =========================================================
with tab_s3:
    st.subheader("SeaweedFS S3 Storage Explorer")
    try:
        s3 = get_s3_client()
        buckets_response = s3.list_buckets()
        buckets = [b["Name"] for b in buckets_response.get("Buckets", [])]

        if not buckets:
            st.warning("No buckets found in SeaweedFS.")
            new_bucket = st.text_input("Create Bucket Name")
            if st.button("Create Bucket", key="btn_create_bucket"):
                if new_bucket.strip():
                    s3.create_bucket(Bucket=new_bucket.strip())
                    st.success(f"Bucket '{new_bucket}' created.")
                    st.rerun()
        else:
            col_b, col_refresh_s3 = st.columns([3, 1])
            with col_b:
                selected_bucket = st.selectbox("Select Bucket", options=buckets, index=0)
            with col_refresh_s3:
                st.write("")
                st.write("")
                if st.button("Refresh Objects", key="btn_refresh_s3"):
                    st.rerun()

            if selected_bucket:
                objects_response = s3.list_objects_v2(Bucket=selected_bucket)
                objects = objects_response.get("Contents", [])

                st.markdown(f"**Bucket:** `{selected_bucket}` | **Total Objects:** `{len(objects)}`")

                if objects:
                    obj_table = []
                    for obj in objects:
                        size_kb = round(obj["Size"] / 1024, 2)
                        obj_table.append({
                            "Key": obj["Key"],
                            "Size (KB)": size_kb,
                            "Last Modified": str(obj["LastModified"]),
                            "ETag": obj.get("ETag", "").strip('"'),
                        })
                    st.dataframe(pd.DataFrame(obj_table), use_container_width=True)

                    st.markdown("#### Inspect & Preview Object")
                    selected_key = st.selectbox("Select Object to Inspect", options=[o["Key"] for o in objects])

                    if selected_key:
                        obj_data = s3.get_object(Bucket=selected_bucket, Key=selected_key)
                        content_type = obj_data.get("ContentType", "application/octet-stream")
                        body_bytes = obj_data["Body"].read()

                        c1, c2 = st.columns(2)
                        c1.markdown(f"**Content-Type:** `{content_type}`")
                        c2.markdown(f"**Size:** `{len(body_bytes)} bytes`")

                        # Preview images
                        if any(selected_key.lower().endswith(ext) for ext in [".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg"]) or "image" in content_type:
                            st.image(body_bytes, caption=selected_key, use_container_width=False)
                        elif "json" in content_type or selected_key.endswith(".json"):
                            try:
                                st.json(json.loads(body_bytes.decode("utf-8")))
                            except Exception:
                                st.code(body_bytes.decode("utf-8", errors="replace"), language="json")
                        elif "text" in content_type or selected_key.endswith(".txt"):
                            st.code(body_bytes.decode("utf-8", errors="replace"), language="text")

                        col_down, col_del = st.columns([1, 1])
                        with col_down:
                            st.download_button(
                                label="Download File",
                                data=body_bytes,
                                file_name=selected_key.split("/")[-1],
                                mime=content_type,
                            )
                        with col_del:
                            if st.button(f"Delete Object", key=f"del_s3_{selected_key}"):
                                s3.delete_object(Bucket=selected_bucket, Key=selected_key)
                                st.success(f"Object `{selected_key}` deleted.")
                                st.rerun()

                else:
                    st.info(f"Bucket '{selected_bucket}' is empty.")

                st.markdown("---")
                st.markdown("#### Upload New File to Bucket")
                uploaded_file = st.file_uploader("Choose file to upload to SeaweedFS", key="s3_uploader")
                target_key = st.text_input("Target Key / Path", value=uploaded_file.name if uploaded_file else "")

                if st.button("Upload to S3", key="btn_upload_s3"):
                    if uploaded_file and target_key:
                        s3.put_object(
                            Bucket=selected_bucket,
                            Key=target_key,
                            Body=uploaded_file.getvalue(),
                            ContentType=uploaded_file.type or "application/octet-stream",
                        )
                        st.success(f"File uploaded successfully to `{selected_bucket}/{target_key}`.")
                        st.rerun()
                    else:
                        st.warning("Please select a file and specify target key.")

    except Exception as e:
        st.error(f"SeaweedFS (S3) connection error: {e}")

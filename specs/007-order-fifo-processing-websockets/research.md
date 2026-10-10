# Phase 0 Research: Asynchronous Order Processing with Redis Streams and WebSockets

## 1. Context and Problem Statement

Under high concurrency (e.g. rush hours with dozens of tables issuing orders via QR codes simultaneously), synchronous write operations on PostgreSQL create row-level locking, deadlocks, and unpredictable latency. Moreover, kitchen staff need real-time push notifications rather than polling HTTP endpoints.

Per **Backend Architectural Inviolable #6** and **ADR-0007**, we implement:
1. **Asynchronous Ingestion**: Ingest order requests via `POST /api/v1/orders`, return `HTTP 202 Accepted` immediately with tracking token/ID.
2. **Sequential FIFO Queuing**: Push incoming orders into Redis Streams partitioned per tenant (`orders:{tenant_slug}`).
3. **Sequential Consumer Worker**: Background service reading from the tenant's stream in strict arrival order, persisting to PostgreSQL with JSONB item snapshots.
4. **Real-Time Push Notifications**: Broadcast an `ORDER_CREATED` event to kitchen displays via WebSockets (`wss://`).

---

## 2. Research Decisions

### Decision 1: Queue Technology: Redis Streams vs RabbitMQ vs PostgreSQL LISTEN/NOTIFY

- **Options Considered**:
  - *Option 1*: RabbitMQ with FIFO queues.
  - *Option 2*: Redis Streams (`XADD`, `XREADGROUP`, `XACK`).
  - *Option 3*: PostgreSQL `LISTEN / NOTIFY`.
- **Decision**: **Option 2 (Redis Streams)**.
- **Rationale**:
  - Redis 7 is already deployed in our container stack (`docker-compose.yml`, port 6379) and used for caching and session state. Adding RabbitMQ introduces heavy operational overhead.
  - Redis Streams provides native persistence, consumer groups, exact message offsets, and strict First-In First-Out (FIFO) sequential processing per key.
  - PostgreSQL `LISTEN/NOTIFY` lacks durable queuing and backpressure.

### Decision 2: Real-Time Protocol: Native ASP.NET Core WebSockets vs SignalR

- **Options Considered**:
  - *Option 1*: Raw ASP.NET Core `WebSocketMiddleware` (`UseWebSockets()`).
  - *Option 2*: ASP.NET Core SignalR Core (`AddSignalR()`).
- **Decision**: **Option 1 (Lightweight Native WebSockets Handler with Token Auth)**.
- **Rationale**:
  - Native WebSockets in ASP.NET Core has zero client library requirements (works out-of-the-box in vanilla browser JS, React, and mobile apps).
  - Minimal memory overhead per connection and strict compliance with the architecture contract specified in ADR-0007 (`wss://`).
  - Manages a thread-safe connection registry per tenant (`ConcurrentDictionary<string, List<WebSocket>>`).

### Decision 3: Idempotency and Stream Acknowledgment

- **Options Considered**:
  - *Option 1*: Fire-and-forget message read.
  - *Option 2*: At-least-once processing with Consumer Group acknowledgment (`XACK`) and persistent Order entity uniqueness.
- **Decision**: **Option 2 (Consumer Group with XACK and Idempotency)**.
- **Rationale**:
  - Prevents order loss if the worker crashes mid-transaction.
  - Each order payload includes a client-generated or server-generated `OrderId`.

### Decision 4: Entity Model and Comanda Snapshot Persistence

- **Options Considered**:
  - *Option 1*: Normalized relational tables (`OrderItems`, `OrderItemModifiers`).
  - *Option 2*: PostgreSQL `JSONB` column `items_snapshot` with total amount and status.
- **Decision**: **Option 2 (JSONB Snapshot aligned with ADR-0003 and ADR-0010)**.
- **Rationale**:
  - Captures immutable prices, allergen data, and modifier names at the exact moment the order was placed, guaranteeing historical accuracy even if catalog items are modified or deleted later.

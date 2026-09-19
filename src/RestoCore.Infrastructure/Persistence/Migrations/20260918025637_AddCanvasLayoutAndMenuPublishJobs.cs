using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCanvasLayoutAndMenuPublishJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentVersionHash",
                table: "tenants",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LayoutConfig",
                table: "tenants",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "menu_publish_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantSlug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VersionHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CdnPurgeRequested = table.Column<bool>(type: "boolean", nullable: false),
                    AssetsQueued = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_publish_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_publish_jobs_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_publish_jobs_TenantId_TriggeredAt",
                table: "menu_publish_jobs",
                columns: new[] { "TenantId", "TriggeredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "menu_publish_jobs");

            migrationBuilder.DropColumn(
                name: "CurrentVersionHash",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "LayoutConfig",
                table: "tenants");
        }
    }
}

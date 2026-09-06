using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomDomain = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BrandingConfig = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableNumber = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tables_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Allergens = table.Column<string[]>(type: "text[]", nullable: false),
                    DietaryLabels = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_items_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_menu_items_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modifier_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    MinSelection = table.Column<int>(type: "integer", nullable: false),
                    MaxSelection = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modifier_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_modifier_groups_menu_items_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modifier_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifierGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PriceDelta = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.00m),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modifier_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_modifier_options_modifier_groups_ModifierGroupId",
                        column: x => x.ModifierGroupId,
                        principalTable: "modifier_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_TenantId_DisplayOrder",
                table: "categories",
                columns: new[] { "TenantId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_CategoryId",
                table: "menu_items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_TenantId_CategoryId_DisplayOrder",
                table: "menu_items",
                columns: new[] { "TenantId", "CategoryId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_TenantId_IsAvailable",
                table: "menu_items",
                columns: new[] { "TenantId", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_modifier_groups_MenuItemId",
                table: "modifier_groups",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_modifier_options_ModifierGroupId",
                table: "modifier_options",
                column: "ModifierGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_tables_TenantId_TableNumber",
                table: "tables",
                columns: new[] { "TenantId", "TableNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tables_Token",
                table: "tables",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_CustomDomain",
                table: "tenants",
                column: "CustomDomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                table: "tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modifier_options");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "modifier_groups");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}

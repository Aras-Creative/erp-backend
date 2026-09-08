using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateStockItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    sku = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    unit = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    costing_method = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    deleted_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_by = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    updated_by = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    deleted_by = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_items", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_active_deleted",
                table: "stock_items",
                columns: new[] { "is_active", "deleted_at" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_name",
                table: "stock_items",
                column: "name"
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_sku_unique",
                table: "stock_items",
                column: "sku",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_items_updated_at",
                table: "stock_items",
                column: "updated_at"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "stock_items");
        }
    }
}

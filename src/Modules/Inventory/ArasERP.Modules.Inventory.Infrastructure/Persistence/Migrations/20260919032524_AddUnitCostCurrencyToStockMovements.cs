using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitCostCurrencyToStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "stock_movements",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "IDR"
            );

            migrationBuilder.AddColumn<decimal>(
                name: "unit_cost",
                table: "stock_movements",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.Sql(
                """
                UPDATE stock_movements AS m
                SET unit_cost = b.unit_cost
                FROM batches AS b
                WHERE m.batch_id = b.id;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "currency", table: "stock_movements");

            migrationBuilder.DropColumn(name: "unit_cost", table: "stock_movements");
        }
    }
}

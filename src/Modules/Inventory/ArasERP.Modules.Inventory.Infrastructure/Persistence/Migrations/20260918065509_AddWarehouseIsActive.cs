using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "warehouses",
                type: "boolean",
                nullable: false,
                defaultValue: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_is_active",
                table: "warehouses",
                column: "is_active"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_warehouses_is_active", table: "warehouses");

            migrationBuilder.DropColumn(name: "is_active", table: "warehouses");
        }
    }
}

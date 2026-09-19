using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRemainingSnakeCaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Name", table: "warehouses", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "warehouses", newName: "id");

            migrationBuilder.RenameColumn(
                name: "FullAddressText",
                table: "warehouses",
                newName: "full_address_text"
            );

            migrationBuilder.RenameIndex(
                name: "IX_warehouses_Name",
                table: "warehouses",
                newName: "IX_warehouses_name"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "name", table: "warehouses", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "warehouses", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "full_address_text",
                table: "warehouses",
                newName: "FullAddressText"
            );

            migrationBuilder.RenameIndex(
                name: "IX_warehouses_name",
                table: "warehouses",
                newName: "IX_warehouses_Name"
            );
        }
    }
}

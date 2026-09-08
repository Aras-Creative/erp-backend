using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWarehouseAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "address_country", table: "warehouses");

            migrationBuilder.DropColumn(name: "address_street", table: "warehouses");

            migrationBuilder.RenameColumn(
                name: "address_state",
                table: "warehouses",
                newName: "address_province_name"
            );

            migrationBuilder.RenameColumn(
                name: "address_city",
                table: "warehouses",
                newName: "address_city_name"
            );

            migrationBuilder.RenameColumn(
                name: "address_postal_code",
                table: "warehouses",
                newName: "address_zip_code"
            );

            migrationBuilder.AddColumn<string>(
                name: "address_sub_district_name",
                table: "warehouses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "address_district_name",
                table: "warehouses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "address_sub_district_name", table: "warehouses");

            migrationBuilder.DropColumn(name: "address_district_name", table: "warehouses");

            migrationBuilder.RenameColumn(
                name: "address_province_name",
                table: "warehouses",
                newName: "address_state"
            );

            migrationBuilder.RenameColumn(
                name: "address_city_name",
                table: "warehouses",
                newName: "address_city"
            );

            migrationBuilder.RenameColumn(
                name: "address_zip_code",
                table: "warehouses",
                newName: "address_postal_code"
            );

            migrationBuilder.AddColumn<string>(
                name: "address_country",
                table: "warehouses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "address_street",
                table: "warehouses",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );
        }
    }
}

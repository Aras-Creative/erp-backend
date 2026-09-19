using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Address.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAddressesIdColumnNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Id", table: "addresses", newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "id", table: "addresses", newName: "Id");
        }
    }
}

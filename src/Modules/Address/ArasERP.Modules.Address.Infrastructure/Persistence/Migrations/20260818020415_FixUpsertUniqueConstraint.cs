using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Address.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUpsertUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_addresses_destination_code_origin_code",
                table: "addresses"
            );

            migrationBuilder.CreateIndex(
                name: "IX_addresses_external_id",
                table: "addresses",
                column: "external_id",
                unique: true,
                filter: "external_id IS NOT NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_addresses_external_id", table: "addresses");

            migrationBuilder.CreateIndex(
                name: "IX_addresses_destination_code_origin_code",
                table: "addresses",
                columns: new[] { "destination_code", "origin_code" },
                unique: true
            );
        }
    }
}

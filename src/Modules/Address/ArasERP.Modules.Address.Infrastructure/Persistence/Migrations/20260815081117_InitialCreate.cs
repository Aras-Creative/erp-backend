using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace ArasERP.Modules.Address.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    origin_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sub_district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, computedColumnSql: "to_tsvector('simple', coalesce(province_name, '') || ' ' || coalesce(city_name, '') || ' ' || coalesce(district_name, '') || ' ' || coalesce(sub_district_name, ''))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_addresses_destination_code_origin_code",
                table: "addresses",
                columns: new[] { "destination_code", "origin_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_addresses_search_vector",
                table: "addresses",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");
        }
    }
}

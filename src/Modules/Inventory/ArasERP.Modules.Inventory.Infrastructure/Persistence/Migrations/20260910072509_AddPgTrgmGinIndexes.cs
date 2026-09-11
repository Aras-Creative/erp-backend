using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPgTrgmGinIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.Sql(
                "CREATE INDEX ix_stock_items_name_trgm ON stock_items USING gin (name gin_trgm_ops);"
            );
            migrationBuilder.Sql(
                "CREATE INDEX ix_stock_items_sku_trgm ON stock_items USING gin (sku gin_trgm_ops);"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ix_stock_items_name_trgm;");
            migrationBuilder.Sql("DROP INDEX ix_stock_items_sku_trgm;");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}

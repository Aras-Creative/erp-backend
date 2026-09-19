using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveOperationalMetadataToStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "recorded_by", table: "batches");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "stock_movements",
                newName: "received_by"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "recorded_by",
                table: "stock_movements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "recorded_by", table: "stock_movements");

            migrationBuilder.RenameColumn(
                name: "received_by",
                table: "stock_movements",
                newName: "created_by"
            );

            migrationBuilder.AddColumn<string>(
                name: "recorded_by",
                table: "batches",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );
        }
    }
}

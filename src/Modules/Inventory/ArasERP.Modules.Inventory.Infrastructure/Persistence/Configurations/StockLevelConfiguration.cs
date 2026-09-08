using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockLevels;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("stock_levels");

        builder.HasKey(l => l.Id);
        builder
            .Property(l => l.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new StockLevelId(value));

        builder
            .Property(l => l.ItemId)
            .HasColumnName("item_id")
            .IsRequired()
            .HasConversion(id => id.Value, value => new StockItemId(value));

        builder
            .Property(l => l.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired()
            .HasConversion(id => id.Value, value => new WarehouseId(value));

        builder
            .Property(l => l.OnHandQty)
            .HasColumnName("on_hand_qty")
            .IsRequired()
            .HasPrecision(18, 4);

        builder
            .Property(l => l.ReservedQty)
            .HasColumnName("reserved_qty")
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(l => l.RowVersion).HasColumnName("row_version").IsRowVersion();

        builder
            .Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(l => l.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .HasIndex(l => new { l.ItemId, l.WarehouseId })
            .IsUnique()
            .HasDatabaseName("ix_stock_levels_item_warehouse_unique");

        builder.HasIndex(l => l.WarehouseId).HasDatabaseName("ix_stock_levels_warehouse_id");
    }
}

using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockMovements;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(m => m.Id);
        builder
            .Property(m => m.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new StockMovementId(value));

        builder
            .Property(m => m.ItemId)
            .HasColumnName("item_id")
            .IsRequired()
            .HasConversion(id => id.Value, value => new StockItemId(value));

        builder
            .Property(m => m.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired()
            .HasConversion(id => id.Value, value => new WarehouseId(value));

        builder
            .Property(m => m.BatchId)
            .HasColumnName("batch_id")
            .IsRequired(false)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value.HasValue ? new BatchId(value.Value) : null
            );

        builder
            .Property(m => m.Direction)
            .HasColumnName("direction")
            .IsRequired()
            .HasConversion(direction => direction.Value, value => Direction.FromValue(value));

        builder
            .Property(m => m.Quantity)
            .HasColumnName("quantity")
            .IsRequired()
            .HasPrecision(18, 4);

        builder
            .Property(m => m.UnitCost)
            .HasColumnName("unit_cost")
            .IsRequired()
            .HasPrecision(18, 4);

        builder
            .Property(m => m.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("IDR");

        builder
            .Property(m => m.SourceType)
            .HasColumnName("source_type")
            .IsRequired()
            .HasConversion(sourceType => sourceType.Value, value => SourceType.FromValue(value));

        builder
            .Property(m => m.SourceReferenceId)
            .HasColumnName("source_reference_id")
            .IsRequired(false);

        builder
            .Property(m => m.ExternalReferenceNo)
            .HasColumnName("external_reference_no")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(m => m.Note).HasColumnName("note").HasMaxLength(500).IsRequired(false);

        builder
            .Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.RecordedBy).HasColumnName("recorded_by").IsRequired();

        builder
            .Property(m => m.ReceivedBy)
            .HasColumnName("received_by")
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .HasIndex(m => new { m.ItemId, m.CreatedAt })
            .HasDatabaseName("ix_stock_movements_item_created");

        builder
            .HasIndex(m => new { m.WarehouseId, m.CreatedAt })
            .HasDatabaseName("ix_stock_movements_warehouse_created");

        builder
            .HasIndex(m => m.SourceReferenceId)
            .HasDatabaseName("ix_stock_movements_source_reference");
    }
}

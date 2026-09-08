using ArasERP.Modules.Inventory.Domain.Batches;
using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("batches");

        builder.HasKey(b => b.Id);
        builder
            .Property(b => b.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new BatchId(value));

        builder
            .Property(b => b.ItemId)
            .HasColumnName("item_id")
            .IsRequired()
            .HasConversion(id => id.Value, value => new StockItemId(value));

        builder
            .Property(b => b.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired()
            .HasConversion(id => id.Value, value => new WarehouseId(value));

        builder.Property(b => b.ReceivedAt).HasColumnName("received_at").IsRequired();

        builder
            .Property(b => b.ReceivedQty)
            .HasColumnName("received_qty")
            .IsRequired()
            .HasPrecision(18, 4);

        builder
            .Property(b => b.RemainingQty)
            .HasColumnName("remaining_qty")
            .IsRequired()
            .HasPrecision(18, 4);

        builder
            .Property(b => b.UnitCost)
            .HasColumnName("unit_cost")
            .IsRequired()
            .HasPrecision(18, 4);

        builder
            .Property(b => b.Status)
            .HasColumnName("batch_status")
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .HasConversion(status => status.Value, value => BatchStatus.FromValue(value));

        builder
            .Property(b => b.ReceiptNumber)
            .HasColumnName("receipt_number")
            .IsRequired()
            .HasMaxLength(50);

        builder
            .Property(b => b.RecordedBy)
            .HasColumnName("recorded_by")
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at").IsRequired(false);

        builder
            .HasIndex(b => b.ReceiptNumber)
            .IsUnique()
            .HasDatabaseName("ix_batches_receipt_number_unique");

        builder
            .HasIndex(b => new { b.ItemId, b.WarehouseId })
            .HasDatabaseName("ix_batches_item_warehouse");

        builder.HasIndex(b => b.ReceivedAt).HasDatabaseName("ix_batches_received_at");
    }
}

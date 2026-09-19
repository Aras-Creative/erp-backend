using ArasERP.Modules.Inventory.Domain.StockItems;
using ArasERP.Modules.Inventory.Domain.StockItems.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        // Primary Key
        builder.HasKey(k => k.Id);
        builder
            .Property(s => s.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new StockItemId(value));

        // Properties
        builder.Property(s => s.Name).HasColumnName("name").IsRequired().HasMaxLength(150);

        builder.Property(s => s.Sku).HasColumnName("sku").IsRequired().HasMaxLength(50);

        builder.HasIndex(s => s.Sku).IsUnique().HasDatabaseName("ix_stock_items_sku_unique"); // Optional: nama index custom

        builder.Property(s => s.Unit).HasColumnName("unit").IsRequired().HasMaxLength(50);

        builder
            .Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder
            .Property(s => s.CostingMethod)
            .HasColumnName("costing_method")
            .IsRequired()
            .HasConversion(cm => cm.Value, value => CostingMethod.FromValue(value));

        // Timestamps
        builder
            .Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at").IsRequired(false);

        // Audit Trail
        builder
            .Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(s => s.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(s => s.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(100)
            .IsRequired(false);

        // Soft Delete Filter
        builder.HasQueryFilter(s => s.DeletedAt == null);

        // Indexes for Performance
        builder.HasIndex(s => s.Name).HasDatabaseName("ix_stock_items_name");

        builder
            .HasIndex(s => new { s.IsActive, s.DeletedAt })
            .HasDatabaseName("ix_stock_items_active_deleted");

        builder.HasIndex(s => s.UpdatedAt).HasDatabaseName("ix_stock_items_updated_at");
    }
}

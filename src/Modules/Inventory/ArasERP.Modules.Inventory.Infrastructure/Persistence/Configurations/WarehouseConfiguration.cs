using ArasERP.Modules.Inventory.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArasERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).HasConversion(id => id.Value, value => new WarehouseId(value));

        builder.Property(w => w.Name).IsRequired().HasMaxLength(150);

        builder.HasIndex(w => w.Name).IsUnique();

        builder.Property(w => w.IsDeleted).HasColumnName("is_deleted").IsRequired();

        builder.Property(w => w.DeletedAtUtc).HasColumnName("deleted_at_utc");

        builder.HasQueryFilter(w => !w.IsDeleted);

        builder.OwnsOne(
            w => w.PersonInCharge,
            personInCharge =>
            {
                personInCharge
                    .Property(p => p.Name)
                    .HasColumnName("person_in_charge_name")
                    .IsRequired()
                    .HasMaxLength(100);

                personInCharge
                    .Property(p => p.Phone)
                    .HasColumnName("person_in_charge_phone")
                    .HasMaxLength(30);
            }
        );

        builder.OwnsOne(
            w => w.Address,
            address =>
            {
                address
                    .Property(a => a.Street)
                    .HasColumnName("address_street")
                    .IsRequired()
                    .HasMaxLength(255);

                address
                    .Property(a => a.City)
                    .HasColumnName("address_city")
                    .IsRequired()
                    .HasMaxLength(100);

                address
                    .Property(a => a.State)
                    .HasColumnName("address_state")
                    .IsRequired()
                    .HasMaxLength(100);

                address
                    .Property(a => a.PostalCode)
                    .HasColumnName("address_postal_code")
                    .IsRequired()
                    .HasMaxLength(20);

                address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100);
            }
        );

        builder.Property(w => w.FullAddressText).HasMaxLength(500);
    }
}

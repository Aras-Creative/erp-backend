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

        builder.Property(w => w.Id).HasColumnName("id").HasConversion(id => id.Value, value => new WarehouseId(value));

        builder.Property(w => w.Name).HasColumnName("name").IsRequired().HasMaxLength(150);

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
                    .Property(a => a.SubDistrictName)
                    .HasColumnName("address_sub_district_name")
                    .IsRequired()
                    .HasMaxLength(100);

                address
                    .Property(a => a.DistrictName)
                    .HasColumnName("address_district_name")
                    .IsRequired()
                    .HasMaxLength(100);

                address
                    .Property(a => a.CityName)
                    .HasColumnName("address_city_name")
                    .IsRequired()
                    .HasMaxLength(100);

                address
                    .Property(a => a.ProvinceName)
                    .HasColumnName("address_province_name")
                    .IsRequired()
                    .HasMaxLength(100);

                address
                    .Property(a => a.ZipCode)
                    .HasColumnName("address_zip_code")
                    .IsRequired()
                    .HasMaxLength(20);
            }
        );

        builder.Property(w => w.FullAddressText).HasColumnName("full_address_text").HasMaxLength(500);
    }
}

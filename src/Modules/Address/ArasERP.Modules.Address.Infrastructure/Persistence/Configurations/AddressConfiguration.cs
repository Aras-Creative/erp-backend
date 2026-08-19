using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace ArasERP.Modules.Address.Infrastructure.Persistence.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Domain.Address>
{
    public void Configure(EntityTypeBuilder<Domain.Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id").HasConversion(id => id.Value, value => new Domain.AddressId(value));

        builder.Property(a => a.DestinationCode).HasColumnName("destination_code").IsRequired().HasMaxLength(20);

        builder.Property(a => a.OriginCode).HasColumnName("origin_code").IsRequired().HasMaxLength(20);

        builder.Property(a => a.ProvinceName).HasColumnName("province_name").IsRequired().HasMaxLength(100);

        builder.Property(a => a.CityName).HasColumnName("city_name").IsRequired().HasMaxLength(100);

        builder.Property(a => a.DistrictName).HasColumnName("district_name").IsRequired().HasMaxLength(100);

        builder.Property(a => a.SubDistrictName).HasColumnName("sub_district_name").IsRequired().HasMaxLength(100);

        builder.Property(a => a.ZipCode).HasColumnName("zip_code").IsRequired().HasMaxLength(10);

        builder.Property(a => a.ExternalId).HasColumnName("external_id").HasMaxLength(100);

        builder.Property(a => a.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Property(a => a.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();

        builder.HasIndex(a => a.ExternalId).IsUnique().HasFilter("external_id IS NOT NULL");

        builder
            .Property<NpgsqlTsVector>("SearchVector")
            .HasColumnName("search_vector")
            .HasComputedColumnSql(
                "to_tsvector('simple', "
                    + "coalesce(province_name, '') || ' ' || "
                    + "coalesce(city_name, '') || ' ' || "
                    + "coalesce(district_name, '') || ' ' || "
                    + "coalesce(sub_district_name, ''))",
                stored: true
            );

        builder.HasIndex("SearchVector").HasMethod("gin");
    }
}

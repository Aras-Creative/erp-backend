using Microsoft.EntityFrameworkCore;

namespace ArasERP.Modules.Address.Infrastructure.Persistence;

public sealed class AddressDbContext : DbContext
{
    public AddressDbContext(DbContextOptions<AddressDbContext> options)
        : base(options) { }

    public DbSet<Domain.Address> Addresses => Set<Domain.Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AddressDbContext).Assembly);
    }
}

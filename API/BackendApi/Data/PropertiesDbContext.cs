using Microsoft.EntityFrameworkCore;
using BackendApi.Entities;

namespace BackendApi.Data;

public sealed class PropertiesDbContext(DbContextOptions<PropertiesDbContext> options)
    : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropertiesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using Properties.Entities;

namespace Properties.Data;

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

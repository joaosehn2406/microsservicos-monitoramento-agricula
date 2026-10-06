using Microsoft.EntityFrameworkCore;
using Storage.Entities;

namespace Storage.Data;

public sealed class StorageDbContext(DbContextOptions<StorageDbContext> options)
    : DbContext(options)
{
    public DbSet<WeatherReading> WeatherReadings => Set<WeatherReading>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StorageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

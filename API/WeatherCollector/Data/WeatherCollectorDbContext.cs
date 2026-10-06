using Microsoft.EntityFrameworkCore;
using WeatherCollector.Entities;

namespace WeatherCollector.Data;

public sealed class WeatherCollectorDbContext(DbContextOptions<WeatherCollectorDbContext> options)
    : DbContext(options)
{
    public DbSet<CollectionExecution> CollectionExecutions => Set<CollectionExecution>();
}

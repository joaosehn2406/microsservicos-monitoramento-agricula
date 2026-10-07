using Microsoft.EntityFrameworkCore;
using BackendApi.Entities;

namespace BackendApi.Data;

public sealed class BackendDbContext(DbContextOptions<BackendDbContext> options)
    : DbContext(options)
{
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<WeatherReading> WeatherReadings => Set<WeatherReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BackendDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

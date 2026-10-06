using Analytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Data;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
    : DbContext(options)
{
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();
}

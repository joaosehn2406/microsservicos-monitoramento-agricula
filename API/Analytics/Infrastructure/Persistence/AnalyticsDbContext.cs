using Microsoft.EntityFrameworkCore;
using Analytics.Domain;

namespace Analytics.Infrastructure.Persistence;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
    : DbContext(options)
{
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertRule>(builder =>
        {
            builder.ToTable("alert_rules");
            builder.HasKey(rule => rule.Id);

            builder.Property(rule => rule.Id).HasColumnName("id");
            builder.Property(rule => rule.PropertyId).HasColumnName("property_id");
            builder.Property(rule => rule.Variable).HasColumnName("variable");
            builder.Property(rule => rule.Operator).HasColumnName("operator");
            builder.Property(rule => rule.Threshold).HasColumnName("threshold");
            builder.Property(rule => rule.Severity).HasColumnName("severity");
            builder.Property(rule => rule.IsActive).HasColumnName("is_active");
            builder.Property(rule => rule.CreatedAt).HasColumnName("created_at");
            builder.Property(rule => rule.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Alert>(builder =>
        {
            builder.ToTable("alerts");
            builder.HasKey(alert => alert.Id);

            builder.Property(alert => alert.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(alert => alert.EventId).HasColumnName("event_id");
            builder.Property(alert => alert.SourceEventId).HasColumnName("source_event_id");
            builder.Property(alert => alert.RuleId).HasColumnName("rule_id");
            builder.Property(alert => alert.PropertyId).HasColumnName("property_id");
            builder.Property(alert => alert.PropertyName).HasColumnName("property_name");
            builder.Property(alert => alert.Variable).HasColumnName("variable");
            builder.Property(alert => alert.Operator).HasColumnName("operator");
            builder.Property(alert => alert.Threshold).HasColumnName("threshold");
            builder.Property(alert => alert.MeasuredValue).HasColumnName("measured_value");
            builder.Property(alert => alert.Severity).HasColumnName("severity");
            builder.Property(alert => alert.ObservedAt).HasColumnName("observed_at");
            builder.Property(alert => alert.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            builder.HasIndex(alert => alert.EventId).IsUnique().HasDatabaseName("uq_alerts_event_id");
            builder.HasIndex(alert => new { alert.SourceEventId, alert.RuleId }).IsUnique()
                .HasDatabaseName("uq_alerts_source_event_id_rule_id");
            builder.HasOne<AlertRule>().WithMany().HasForeignKey(alert => alert.RuleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProcessedEvent>(builder =>
        {
            builder.ToTable("processed_events");
            builder.HasKey(processed => new { processed.EventId, processed.Consumer }).HasName("pk_processed_events");

            builder.Property(processed => processed.EventId).HasColumnName("event_id");
            builder.Property(processed => processed.Consumer).HasColumnName("consumer");
            builder.Property(processed => processed.ProcessedAt).HasColumnName("processed_at").HasDefaultValueSql("now()");
        });

        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using MonitoringApi.Entities;

namespace MonitoringApi.Infrastructure.Persistence;

public sealed class MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
    : DbContext(options)
{
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<WeatherReading> WeatherReadings => Set<WeatherReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertRule>(builder =>
        {
            builder.ToTable("alert_rules");
            builder.HasKey(rule => rule.Id);

            builder.Property(rule => rule.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(rule => rule.PropertyId).HasColumnName("property_id");
            builder.Property(rule => rule.Variable).HasColumnName("variable");
            builder.Property(rule => rule.Operator).HasColumnName("operator");
            builder.Property(rule => rule.Threshold).HasColumnName("threshold");
            builder.Property(rule => rule.Severity).HasColumnName("severity");
            builder.Property(rule => rule.IsActive).HasColumnName("is_active");
            builder.Property(rule => rule.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            builder.Property(rule => rule.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<WeatherReading>(builder =>
        {
            builder.ToTable("weather_readings");
            builder.HasKey(reading => reading.Id);

            builder.Property(reading => reading.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(reading => reading.EventId).HasColumnName("event_id");
            builder.Property(reading => reading.ClientId).HasColumnName("client_id");
            builder.Property(reading => reading.PropertyId).HasColumnName("property_id");
            builder.Property(reading => reading.PropertyName).HasColumnName("property_name");
            builder.Property(reading => reading.Latitude).HasColumnName("latitude");
            builder.Property(reading => reading.Longitude).HasColumnName("longitude");
            builder.Property(reading => reading.ObservedAt).HasColumnName("observed_at");
            builder.Property(reading => reading.CollectedAt).HasColumnName("collected_at");
            builder.Property(reading => reading.TemperatureCelsius).HasColumnName("temperature_celsius");
            builder.Property(reading => reading.RelativeHumidityPercent).HasColumnName("relative_humidity_percent");
            builder.Property(reading => reading.PrecipitationMm).HasColumnName("precipitation_mm");
            builder.Property(reading => reading.SoilMoisture).HasColumnName("soil_moisture");
            builder.Property(reading => reading.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            builder.HasIndex(reading => reading.EventId).IsUnique().HasDatabaseName("uq_weather_readings_event_id");
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

        modelBuilder.Entity<Notification>(builder =>
        {
            builder.ToTable("notifications");
            builder.HasKey(notification => notification.Id);

            builder.Property(notification => notification.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(notification => notification.AlertId).HasColumnName("alert_id");
            builder.Property(notification => notification.AlertEventId).HasColumnName("alert_event_id");
            builder.Property(notification => notification.PropertyId).HasColumnName("property_id");
            builder.Property(notification => notification.Severity).HasColumnName("severity");
            builder.Property(notification => notification.Title).HasColumnName("title");
            builder.Property(notification => notification.Message).HasColumnName("message");
            builder.Property(notification => notification.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            builder.HasIndex(notification => notification.AlertEventId).IsUnique()
                .HasDatabaseName("uq_notifications_alert_event_id");
        });

        base.OnModelCreating(modelBuilder);
    }
}

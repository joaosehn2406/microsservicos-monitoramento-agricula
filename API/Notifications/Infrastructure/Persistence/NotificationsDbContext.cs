using Microsoft.EntityFrameworkCore;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

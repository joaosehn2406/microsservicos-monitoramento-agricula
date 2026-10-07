using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BackendApi.Entities;

namespace BackendApi.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
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
    }
}

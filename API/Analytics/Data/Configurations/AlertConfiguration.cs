using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Data.Configurations;

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
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
    }
}

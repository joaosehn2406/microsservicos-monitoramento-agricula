using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Data.Configurations;

public sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
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
    }
}

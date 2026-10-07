using BackendApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendApi.Data.Configurations;

public sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
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
    }
}

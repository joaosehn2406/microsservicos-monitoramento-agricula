using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Analytics.Entities;

namespace Analytics.Data.Configurations;

public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");
        builder.HasKey(processed => new { processed.EventId, processed.Consumer }).HasName("pk_processed_events");

        builder.Property(processed => processed.EventId).HasColumnName("event_id");
        builder.Property(processed => processed.Consumer).HasColumnName("consumer");
        builder.Property(processed => processed.ProcessedAt).HasColumnName("processed_at").HasDefaultValueSql("now()");
    }
}

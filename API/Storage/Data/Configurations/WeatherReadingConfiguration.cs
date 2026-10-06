using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Entities;

namespace Storage.Data.Configurations;

public sealed class WeatherReadingConfiguration : IEntityTypeConfiguration<WeatherReading>
{
    public void Configure(EntityTypeBuilder<WeatherReading> builder)
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
    }
}

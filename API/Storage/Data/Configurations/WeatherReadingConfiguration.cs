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
    }
}

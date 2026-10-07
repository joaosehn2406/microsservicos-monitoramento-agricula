using BackendApi.Data;
using BackendApi.DTOs.Requests;
using BackendApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Repositories;

public sealed class WeatherReadingRepository(BackendDbContext context) : IWeatherReadingRepository
{
    public Task<(IReadOnlyList<WeatherReading> Items, int TotalItems)> ListAsync(
        ReadingQuery query,
        CancellationToken cancellationToken)
    {
        var readings = context.WeatherReadings.AsNoTracking();

        if (query.PropertyId is { } propertyId) readings = readings.Where(reading => reading.PropertyId == propertyId);
        if (query.From is { } from) readings = readings.Where(reading => reading.ObservedAt >= from.UtcDateTime);
        if (query.To is { } to) readings = readings.Where(reading => reading.ObservedAt <= to.UtcDateTime);

        return readings
            .OrderByDescending(reading => reading.ObservedAt)
            .ThenByDescending(reading => reading.Id)
            .ToPageAsync(query, cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherReading>> ListLatestAsync(
        Guid? propertyId,
        CancellationToken cancellationToken)
    {
        var readings = context.WeatherReadings.AsNoTracking();

        if (propertyId is { } id) readings = readings.Where(reading => reading.PropertyId == id);

        // Repeated readings share observed_at (Open-Meteo updates every ~15 min): the last stored one wins.
        var latest = await readings
            .GroupBy(reading => reading.PropertyId)
            .Select(group => group
                .OrderByDescending(reading => reading.ObservedAt)
                .ThenByDescending(reading => reading.CreatedAt)
                .First())
            .ToListAsync(cancellationToken);

        return latest.OrderBy(reading => reading.PropertyName).ToList();
    }
}

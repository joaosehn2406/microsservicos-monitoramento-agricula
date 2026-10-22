using Microsoft.EntityFrameworkCore;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Contracts.Responses;
using MonitoringApi.Infrastructure.Persistence;

using MonitoringApi.Entities;

namespace MonitoringApi.Application;

public sealed class WeatherReadingService(MonitoringDbContext context)
{
    public async Task<PagedResponse<WeatherReading>> ListAsync(
        ReadingQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddPageErrors(query);
        errors.AddRangeErrors(query.From, query.To);
        errors.ThrowIfAny();

        var readings = context.WeatherReadings.AsNoTracking();
        if (query.PropertyId is { } propertyId) readings = readings.Where(reading => reading.PropertyId == propertyId);
        if (query.From is { } from) readings = readings.Where(reading => reading.ObservedAt >= from.UtcDateTime);
        if (query.To is { } to) readings = readings.Where(reading => reading.ObservedAt <= to.UtcDateTime);

        var (items, totalItems) = await readings
            .OrderByDescending(reading => reading.ObservedAt)
            .ThenByDescending(reading => reading.Id)
            .ToPageAsync(query, cancellationToken);
        return PagedResponse<WeatherReading>.Create(
            items, query.Page, query.PageSize, totalItems);
    }

    public async Task<IReadOnlyList<WeatherReading>> ListLatestAsync(
        Guid? propertyId,
        CancellationToken cancellationToken)
    {
        var readings = context.WeatherReadings.AsNoTracking();
        if (propertyId is { } id) readings = readings.Where(reading => reading.PropertyId == id);

        var latest = await readings
            .GroupBy(reading => reading.PropertyId)
            .Select(group => group
                .OrderByDescending(reading => reading.ObservedAt)
                .ThenByDescending(reading => reading.CreatedAt)
                .First())
            .ToListAsync(cancellationToken);

        return latest.OrderBy(reading => reading.PropertyName)
            .ToList();
    }
}

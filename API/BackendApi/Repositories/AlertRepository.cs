using BackendApi.Data;
using BackendApi.DTOs.Requests;
using BackendApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Repositories;

public sealed class AlertRepository(BackendDbContext context) : IAlertRepository
{
    public Task<(IReadOnlyList<Alert> Items, int TotalItems)> ListAsync(
        AlertQuery query,
        CancellationToken cancellationToken)
    {
        var alerts = context.Alerts.AsNoTracking();

        if (query.PropertyId is { } propertyId) alerts = alerts.Where(alert => alert.PropertyId == propertyId);
        if (query.Variable is { } variable) alerts = alerts.Where(alert => alert.Variable == variable);
        if (query.Severity is { } severity) alerts = alerts.Where(alert => alert.Severity == severity);
        if (query.From is { } from) alerts = alerts.Where(alert => alert.CreatedAt >= from.UtcDateTime);
        if (query.To is { } to) alerts = alerts.Where(alert => alert.CreatedAt <= to.UtcDateTime);

        return alerts
            .OrderByDescending(alert => alert.CreatedAt)
            .ThenByDescending(alert => alert.Id)
            .ToPageAsync(query, cancellationToken);
    }

    public Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Alerts.AsNoTracking().SingleOrDefaultAsync(alert => alert.Id == id, cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using MonitoringApi.Application.Exceptions;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Contracts.Responses;
using MonitoringApi.Infrastructure.Persistence;

using MonitoringApi.Entities;

namespace MonitoringApi.Application;

public sealed class AlertService(MonitoringDbContext context)
{
    public async Task<PagedResponse<Alert>> ListAsync(AlertQuery query, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddPageErrors(query);
        errors.AddRangeErrors(query.From, query.To);
        errors.AddIfNotAllowed("variable", query.Variable, DomainValues.Variables);
        errors.AddIfNotAllowed("severity", query.Severity, DomainValues.Severities);
        errors.ThrowIfAny();

        var alerts = context.Alerts.AsNoTracking();
        if (query.PropertyId is { } propertyId) alerts = alerts.Where(alert => alert.PropertyId == propertyId);
        if (query.Variable is { } variable) alerts = alerts.Where(alert => alert.Variable == variable);
        if (query.Severity is { } severity) alerts = alerts.Where(alert => alert.Severity == severity);
        if (query.From is { } from) alerts = alerts.Where(alert => alert.CreatedAt >= from.UtcDateTime);
        if (query.To is { } to) alerts = alerts.Where(alert => alert.CreatedAt <= to.UtcDateTime);

        var (items, totalItems) = await alerts
            .OrderByDescending(alert => alert.CreatedAt)
            .ThenByDescending(alert => alert.Id)
            .ToPageAsync(query, cancellationToken);
        return PagedResponse<Alert>.Create(
            items, query.Page, query.PageSize, totalItems);
    }

    public async Task<Alert> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var alert = await context.Alerts.AsNoTracking()
                        .SingleOrDefaultAsync(alert => alert.Id == id, cancellationToken)
                    ?? throw new NotFoundException($"Alert {id} was not found.");
        return alert;
    }
}

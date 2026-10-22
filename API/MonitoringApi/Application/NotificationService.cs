using Microsoft.EntityFrameworkCore;
using MonitoringApi.Application.Exceptions;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Contracts.Responses;
using MonitoringApi.Infrastructure.Persistence;

using MonitoringApi.Entities;

namespace MonitoringApi.Application;

public sealed class NotificationService(MonitoringDbContext context)
{
    public async Task<PagedResponse<Notification>> ListAsync(
        NotificationQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddPageErrors(query);
        errors.AddRangeErrors(query.From, query.To);
        errors.AddIfNotAllowed("severity", query.Severity, DomainValues.Severities);
        errors.ThrowIfAny();

        var notifications = context.Notifications.AsNoTracking();
        if (query.PropertyId is { } propertyId)
            notifications = notifications.Where(notification => notification.PropertyId == propertyId);
        if (query.Severity is { } severity)
            notifications = notifications.Where(notification => notification.Severity == severity);
        if (query.From is { } from)
            notifications = notifications.Where(notification => notification.CreatedAt >= from.UtcDateTime);
        if (query.To is { } to)
            notifications = notifications.Where(notification => notification.CreatedAt <= to.UtcDateTime);

        var (items, totalItems) = await notifications
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .ToPageAsync(query, cancellationToken);
        return PagedResponse<Notification>.Create(
            items, query.Page, query.PageSize, totalItems);
    }

    public async Task<Notification> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications.AsNoTracking()
                               .SingleOrDefaultAsync(notification => notification.Id == id, cancellationToken)
                           ?? throw new NotFoundException($"Notification {id} was not found.");
        return notification;
    }
}

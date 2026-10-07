using BackendApi.Data;
using BackendApi.DTOs.Requests;
using BackendApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Repositories;

public sealed class NotificationRepository(BackendDbContext context) : INotificationRepository
{
    public Task<(IReadOnlyList<Notification> Items, int TotalItems)> ListAsync(
        NotificationQuery query,
        CancellationToken cancellationToken)
    {
        var notifications = context.Notifications.AsNoTracking();

        if (query.PropertyId is { } propertyId)
            notifications = notifications.Where(notification => notification.PropertyId == propertyId);
        if (query.Severity is { } severity)
            notifications = notifications.Where(notification => notification.Severity == severity);
        if (query.From is { } from)
            notifications = notifications.Where(notification => notification.CreatedAt >= from.UtcDateTime);
        if (query.To is { } to)
            notifications = notifications.Where(notification => notification.CreatedAt <= to.UtcDateTime);

        return notifications
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .ToPageAsync(query, cancellationToken);
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Notifications.AsNoTracking()
            .SingleOrDefaultAsync(notification => notification.Id == id, cancellationToken);
}

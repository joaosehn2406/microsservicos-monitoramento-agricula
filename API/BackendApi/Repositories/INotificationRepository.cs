using BackendApi.DTOs.Requests;
using BackendApi.Entities;

namespace BackendApi.Repositories;

public interface INotificationRepository
{
    Task<(IReadOnlyList<Notification> Items, int TotalItems)> ListAsync(
        NotificationQuery query,
        CancellationToken cancellationToken);

    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;

namespace BackendApi.Services;

public interface INotificationService
{
    Task<PagedResponse<NotificationResponse>> ListAsync(NotificationQuery query, CancellationToken cancellationToken);

    Task<NotificationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

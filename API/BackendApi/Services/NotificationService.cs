using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Exceptions;
using BackendApi.Mappers;
using BackendApi.Repositories;

namespace BackendApi.Services;

public sealed class NotificationService(INotificationRepository repository) : INotificationService
{
    public async Task<PagedResponse<NotificationResponse>> ListAsync(
        NotificationQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddPageErrors(query);
        errors.AddRangeErrors(query.From, query.To);
        errors.AddIfNotAllowed("severity", query.Severity, DomainValues.Severities);
        errors.ThrowIfAny();

        var (items, totalItems) = await repository.ListAsync(query, cancellationToken);
        return PagedResponse<NotificationResponse>.Create(
            items.Select(NotificationMapper.ToResponse).ToList(), query.Page, query.PageSize, totalItems);
    }

    public async Task<NotificationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(id, cancellationToken)
                           ?? throw new NotificationNotFoundException(id);
        return NotificationMapper.ToResponse(notification);
    }
}

using BackendApi.DTOs.Responses;
using BackendApi.Entities;

namespace BackendApi.Mappers;

public static class NotificationMapper
{
    public static NotificationResponse ToResponse(Notification notification) => new(
        notification.Id,
        notification.AlertId,
        notification.AlertEventId,
        notification.PropertyId,
        notification.Severity,
        notification.Title,
        notification.Message,
        notification.CreatedAt);
}

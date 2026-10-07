namespace BackendApi.DTOs.Responses;

public sealed record NotificationResponse(
    Guid Id,
    Guid AlertId,
    Guid AlertEventId,
    Guid PropertyId,
    string Severity,
    string Title,
    string Message,
    DateTime CreatedAt);

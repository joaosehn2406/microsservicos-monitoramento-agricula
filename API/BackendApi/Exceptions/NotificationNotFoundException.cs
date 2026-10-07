namespace BackendApi.Exceptions;

public sealed class NotificationNotFoundException(Guid id) : NotFoundException($"Notification {id} was not found.");

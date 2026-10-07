namespace BackendApi.Exceptions;

public sealed class AlertNotFoundException(Guid id) : NotFoundException($"Alert {id} was not found.");

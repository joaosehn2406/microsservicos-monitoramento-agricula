namespace BackendApi.Exceptions;

public sealed class AlertRuleNotFoundException(Guid id) : NotFoundException($"Alert rule {id} was not found.");

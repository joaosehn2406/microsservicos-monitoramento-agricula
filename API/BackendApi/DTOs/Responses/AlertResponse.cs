namespace BackendApi.DTOs.Responses;

/// <summary><c>RuleId</c> is null once the rule has been deleted.</summary>
public sealed record AlertResponse(
    Guid Id,
    Guid EventId,
    Guid SourceEventId,
    Guid? RuleId,
    Guid PropertyId,
    string PropertyName,
    string Variable,
    string Operator,
    double Threshold,
    double MeasuredValue,
    string Severity,
    DateTime ObservedAt,
    DateTime CreatedAt);

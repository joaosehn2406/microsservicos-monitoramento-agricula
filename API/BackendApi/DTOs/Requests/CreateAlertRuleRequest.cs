namespace BackendApi.DTOs.Requests;

/// <summary>Members are nullable so a missing field is reported as a validation error.</summary>
public sealed record CreateAlertRuleRequest(
    Guid? PropertyId,
    string? Variable,
    string? Operator,
    double? Threshold,
    string? Severity,
    bool? IsActive);

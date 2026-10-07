namespace BackendApi.DTOs.Requests;

/// <summary>Replaces every field of the rule; members are nullable so a missing field is reported.</summary>
public sealed record UpdateAlertRuleRequest(
    Guid? PropertyId,
    string? Variable,
    string? Operator,
    double? Threshold,
    string? Severity,
    bool? IsActive);

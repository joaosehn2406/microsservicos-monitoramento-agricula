namespace MonitoringApi.Contracts.Requests;

public sealed record UpdateAlertRuleRequest(
    Guid? PropertyId,
    string? Variable,
    string? Operator,
    double? Threshold,
    string? Severity,
    bool? IsActive);

namespace BackendApi.DTOs.Responses;

public sealed record AlertRuleResponse(
    Guid Id,
    Guid PropertyId,
    string Variable,
    string Operator,
    double Threshold,
    string Severity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

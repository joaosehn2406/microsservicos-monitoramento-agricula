using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Entities;

namespace BackendApi.Mappers;

/// <summary>Requests must be validated before mapping.</summary>
public static class AlertRuleMapper
{
    public static AlertRule ToEntity(CreateAlertRuleRequest request) => new()
    {
        Id = Guid.CreateVersion7(),
        PropertyId = request.PropertyId!.Value,
        Variable = request.Variable!,
        Operator = request.Operator!,
        Threshold = request.Threshold!.Value,
        Severity = request.Severity!,
        IsActive = request.IsActive ?? true
    };

    public static void Apply(UpdateAlertRuleRequest request, AlertRule rule)
    {
        rule.PropertyId = request.PropertyId!.Value;
        rule.Variable = request.Variable!;
        rule.Operator = request.Operator!;
        rule.Threshold = request.Threshold!.Value;
        rule.Severity = request.Severity!;
        rule.IsActive = request.IsActive!.Value;
    }

    public static AlertRuleResponse ToResponse(AlertRule rule) => new(
        rule.Id,
        rule.PropertyId,
        rule.Variable,
        rule.Operator,
        rule.Threshold,
        rule.Severity,
        rule.IsActive,
        rule.CreatedAt,
        rule.UpdatedAt);
}

using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Exceptions;
using BackendApi.Mappers;
using BackendApi.Repositories;

namespace BackendApi.Services;

public sealed class AlertRuleService(IAlertRuleRepository repository) : IAlertRuleService
{
    public async Task<IReadOnlyList<AlertRuleResponse>> ListAsync(
        AlertRuleQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddIfNotAllowed("variable", query.Variable, DomainValues.Variables);
        errors.ThrowIfAny();

        var rules = await repository.ListAsync(query, cancellationToken);
        return rules.Select(AlertRuleMapper.ToResponse).ToList();
    }

    public async Task<AlertRuleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await repository.GetByIdAsync(id, cancellationToken) ?? throw new AlertRuleNotFoundException(id);
        return AlertRuleMapper.ToResponse(rule);
    }

    public async Task<AlertRuleResponse> CreateAsync(
        CreateAlertRuleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request.PropertyId, request.Variable, request.Operator, request.Threshold, request.Severity,
            request.IsActive, isActiveRequired: false);

        var rule = AlertRuleMapper.ToEntity(request);
        await repository.AddAsync(rule, cancellationToken);
        return AlertRuleMapper.ToResponse(rule);
    }

    public async Task<AlertRuleResponse> UpdateAsync(
        Guid id,
        UpdateAlertRuleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request.PropertyId, request.Variable, request.Operator, request.Threshold, request.Severity,
            request.IsActive, isActiveRequired: true);

        var updated = await repository.UpdateAsync(id, rule => AlertRuleMapper.Apply(request, rule), cancellationToken)
                      ?? throw new AlertRuleNotFoundException(id);
        return AlertRuleMapper.ToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await repository.DeleteAsync(id, cancellationToken))
        {
            throw new AlertRuleNotFoundException(id);
        }
    }

    private static void Validate(
        Guid? propertyId,
        string? variable,
        string? @operator,
        double? threshold,
        string? severity,
        bool? isActive,
        bool isActiveRequired)
    {
        var errors = new ValidationErrors();

        if (propertyId is null || propertyId == Guid.Empty) errors.Add("propertyId", "propertyId is required.");
        if (variable is null) errors.Add("variable", "variable is required.");
        if (@operator is null) errors.Add("operator", "operator is required.");
        if (threshold is null) errors.Add("threshold", "threshold is required.");
        else if (!double.IsFinite(threshold.Value)) errors.Add("threshold", "threshold must be a finite number.");
        if (severity is null) errors.Add("severity", "severity is required.");
        if (isActiveRequired && isActive is null) errors.Add("isActive", "isActive is required.");

        errors.AddIfNotAllowed("variable", variable, DomainValues.Variables);
        errors.AddIfNotAllowed("operator", @operator, DomainValues.Operators);
        errors.AddIfNotAllowed("severity", severity, DomainValues.Severities);

        errors.ThrowIfAny();
    }
}

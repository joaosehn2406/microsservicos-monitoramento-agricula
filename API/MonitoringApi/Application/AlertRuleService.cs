using Microsoft.EntityFrameworkCore;
using MonitoringApi.Application.Exceptions;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Entities;
using MonitoringApi.Infrastructure.Persistence;

namespace MonitoringApi.Application;

public sealed class AlertRuleService(MonitoringDbContext context)
{
    public async Task<IReadOnlyList<AlertRule>> ListAsync(
        AlertRuleQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddIfNotAllowed("variable", query.Variable, DomainValues.Variables);
        errors.ThrowIfAny();

        var rules = context.AlertRules.AsNoTracking();
        if (query.PropertyId is { } propertyId) rules = rules.Where(rule => rule.PropertyId == propertyId);
        if (query.Variable is { } variable) rules = rules.Where(rule => rule.Variable == variable);
        if (query.IsActive is { } isActive) rules = rules.Where(rule => rule.IsActive == isActive);

        var items = await rules
            .OrderBy(rule => rule.PropertyId)
            .ThenBy(rule => rule.Variable)
            .ThenBy(rule => rule.CreatedAt)
            .ToListAsync(cancellationToken);
        return items;
    }

    public async Task<AlertRule> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await context.AlertRules.AsNoTracking()
                       .SingleOrDefaultAsync(rule => rule.Id == id, cancellationToken)
                   ?? throw new NotFoundException($"Alert rule {id} was not found.");
        return rule;
    }

    public async Task<AlertRule> CreateAsync(
        CreateAlertRuleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request.PropertyId, request.Variable, request.Operator, request.Threshold, request.Severity,
            request.IsActive, isActiveRequired: false);

        var rule = new AlertRule
        {
            PropertyId = request.PropertyId!.Value,
            Variable = request.Variable!,
            Operator = request.Operator!,
            Threshold = request.Threshold!.Value,
            Severity = request.Severity!,
            IsActive = request.IsActive ?? true
        };
        context.AlertRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);
        return rule;
    }

    public async Task<AlertRule> UpdateAsync(
        Guid id,
        UpdateAlertRuleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request.PropertyId, request.Variable, request.Operator, request.Threshold, request.Severity,
            request.IsActive, isActiveRequired: true);

        var rule = await context.AlertRules.SingleOrDefaultAsync(rule => rule.Id == id, cancellationToken)
                   ?? throw new NotFoundException($"Alert rule {id} was not found.");
        rule.PropertyId = request.PropertyId!.Value;
        rule.Variable = request.Variable!;
        rule.Operator = request.Operator!;
        rule.Threshold = request.Threshold!.Value;
        rule.Severity = request.Severity!;
        rule.IsActive = request.IsActive!.Value;
        rule.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return rule;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await context.AlertRules.Where(rule => rule.Id == id)
                .ExecuteDeleteAsync(cancellationToken) == 0)
        {
            throw new NotFoundException($"Alert rule {id} was not found.");
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

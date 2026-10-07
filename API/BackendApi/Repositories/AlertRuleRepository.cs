using BackendApi.Data;
using BackendApi.DTOs.Requests;
using BackendApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Repositories;

public sealed class AlertRuleRepository(BackendDbContext context) : IAlertRuleRepository
{
    public async Task<IReadOnlyList<AlertRule>> ListAsync(AlertRuleQuery query, CancellationToken cancellationToken)
    {
        var rules = context.AlertRules.AsNoTracking();

        if (query.PropertyId is { } propertyId) rules = rules.Where(rule => rule.PropertyId == propertyId);
        if (query.Variable is { } variable) rules = rules.Where(rule => rule.Variable == variable);
        if (query.IsActive is { } isActive) rules = rules.Where(rule => rule.IsActive == isActive);

        return await rules
            .OrderBy(rule => rule.PropertyId)
            .ThenBy(rule => rule.Variable)
            .ThenBy(rule => rule.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.AlertRules.AsNoTracking().SingleOrDefaultAsync(rule => rule.Id == id, cancellationToken);

    public async Task AddAsync(AlertRule rule, CancellationToken cancellationToken)
    {
        context.AlertRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AlertRule?> UpdateAsync(Guid id, Action<AlertRule> update, CancellationToken cancellationToken)
    {
        var existing = await context.AlertRules.SingleOrDefaultAsync(rule => rule.Id == id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        update(existing);
        existing.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await context.AlertRules.Where(rule => rule.Id == id).ExecuteDeleteAsync(cancellationToken) > 0;
}

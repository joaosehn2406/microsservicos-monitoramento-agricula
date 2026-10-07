using Analytics.Data;
using Analytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Repositories;

public sealed class AlertRuleRepository(AnalyticsDbContext context) : IAlertRuleRepository
{
    public async Task<IReadOnlyList<AlertRule>> GetActiveByPropertyAsync(
        Guid propertyId,
        CancellationToken cancellationToken) =>
        await context.AlertRules
            .AsNoTracking()
            .Where(rule => rule.PropertyId == propertyId && rule.IsActive)
            .ToListAsync(cancellationToken);
}

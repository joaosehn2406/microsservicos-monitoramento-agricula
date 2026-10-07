using Analytics.Entities;

namespace Analytics.Repositories;

public interface IAlertRuleRepository
{
    Task<IReadOnlyList<AlertRule>> GetActiveByPropertyAsync(Guid propertyId, CancellationToken cancellationToken);
}

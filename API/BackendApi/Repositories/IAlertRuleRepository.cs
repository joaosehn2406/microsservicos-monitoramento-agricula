using BackendApi.DTOs.Requests;
using BackendApi.Entities;

namespace BackendApi.Repositories;

public interface IAlertRuleRepository
{
    Task<IReadOnlyList<AlertRule>> ListAsync(AlertRuleQuery query, CancellationToken cancellationToken);

    Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(AlertRule rule, CancellationToken cancellationToken);

    /// <summary>Applies <paramref name="update"/> and sets updated_at. Returns null when the rule does not exist.</summary>
    Task<AlertRule?> UpdateAsync(Guid id, Action<AlertRule> update, CancellationToken cancellationToken);

    /// <summary>Physical delete; alerts keep rule_id = null through the FK. False when the rule does not exist.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

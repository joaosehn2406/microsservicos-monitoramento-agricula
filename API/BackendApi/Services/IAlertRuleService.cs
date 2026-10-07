using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;

namespace BackendApi.Services;

public interface IAlertRuleService
{
    Task<IReadOnlyList<AlertRuleResponse>> ListAsync(AlertRuleQuery query, CancellationToken cancellationToken);

    Task<AlertRuleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AlertRuleResponse> CreateAsync(CreateAlertRuleRequest request, CancellationToken cancellationToken);

    Task<AlertRuleResponse> UpdateAsync(Guid id, UpdateAlertRuleRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

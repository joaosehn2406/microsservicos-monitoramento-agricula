using BackendApi.DTOs.Requests;
using BackendApi.Entities;

namespace BackendApi.Repositories;

public interface IAlertRepository
{
    Task<(IReadOnlyList<Alert> Items, int TotalItems)> ListAsync(AlertQuery query, CancellationToken cancellationToken);

    Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

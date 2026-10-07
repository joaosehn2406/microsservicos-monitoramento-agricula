using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;

namespace BackendApi.Services;

public interface IAlertService
{
    Task<PagedResponse<AlertResponse>> ListAsync(AlertQuery query, CancellationToken cancellationToken);

    Task<AlertResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

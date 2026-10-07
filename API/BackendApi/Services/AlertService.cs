using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Exceptions;
using BackendApi.Mappers;
using BackendApi.Repositories;

namespace BackendApi.Services;

public sealed class AlertService(IAlertRepository repository) : IAlertService
{
    public async Task<PagedResponse<AlertResponse>> ListAsync(AlertQuery query, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddPageErrors(query);
        errors.AddRangeErrors(query.From, query.To);
        errors.AddIfNotAllowed("variable", query.Variable, DomainValues.Variables);
        errors.AddIfNotAllowed("severity", query.Severity, DomainValues.Severities);
        errors.ThrowIfAny();

        var (items, totalItems) = await repository.ListAsync(query, cancellationToken);
        return PagedResponse<AlertResponse>.Create(
            items.Select(AlertMapper.ToResponse).ToList(), query.Page, query.PageSize, totalItems);
    }

    public async Task<AlertResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var alert = await repository.GetByIdAsync(id, cancellationToken) ?? throw new AlertNotFoundException(id);
        return AlertMapper.ToResponse(alert);
    }
}

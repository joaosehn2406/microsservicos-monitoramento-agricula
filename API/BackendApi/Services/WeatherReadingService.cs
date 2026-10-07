using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Mappers;
using BackendApi.Repositories;

namespace BackendApi.Services;

public sealed class WeatherReadingService(IWeatherReadingRepository repository) : IWeatherReadingService
{
    public async Task<PagedResponse<WeatherReadingResponse>> ListAsync(
        ReadingQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        errors.AddPageErrors(query);
        errors.AddRangeErrors(query.From, query.To);
        errors.ThrowIfAny();

        var (items, totalItems) = await repository.ListAsync(query, cancellationToken);
        return PagedResponse<WeatherReadingResponse>.Create(
            items.Select(WeatherReadingMapper.ToResponse).ToList(), query.Page, query.PageSize, totalItems);
    }

    public async Task<IReadOnlyList<WeatherReadingResponse>> ListLatestAsync(
        Guid? propertyId,
        CancellationToken cancellationToken)
    {
        var readings = await repository.ListLatestAsync(propertyId, cancellationToken);
        return readings.Select(WeatherReadingMapper.ToResponse).ToList();
    }
}

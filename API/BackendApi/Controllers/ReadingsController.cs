using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/readings")]
public sealed class ReadingsController(IWeatherReadingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WeatherReadingResponse>>> List(
        [FromQuery] ReadingQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("latest")]
    public async Task<ActionResult<IReadOnlyList<WeatherReadingResponse>>> ListLatest(
        [FromQuery] Guid? propertyId,
        CancellationToken cancellationToken) =>
        Ok(await service.ListLatestAsync(propertyId, cancellationToken));
}

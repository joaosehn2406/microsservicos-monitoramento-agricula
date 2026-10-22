using Microsoft.AspNetCore.Mvc;
using MonitoringApi.Application;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Contracts.Responses;

using MonitoringApi.Entities;

namespace MonitoringApi.Controllers;

[ApiController]
[Route("api/readings")]
public sealed class ReadingsController(WeatherReadingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WeatherReading>>> List(
        [FromQuery] ReadingQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("latest")]
    public async Task<ActionResult<IReadOnlyList<WeatherReading>>> ListLatest(
        [FromQuery] Guid? propertyId,
        CancellationToken cancellationToken) =>
        Ok(await service.ListLatestAsync(propertyId, cancellationToken));
}

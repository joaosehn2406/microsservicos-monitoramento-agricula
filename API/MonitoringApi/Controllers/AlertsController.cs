using Microsoft.AspNetCore.Mvc;
using MonitoringApi.Application;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Contracts.Responses;

using MonitoringApi.Entities;

namespace MonitoringApi.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController(AlertService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<Alert>>> List(
        [FromQuery] AlertQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Alert>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));
}

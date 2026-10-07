using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController(IAlertService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AlertResponse>>> List(
        [FromQuery] AlertQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlertResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));
}

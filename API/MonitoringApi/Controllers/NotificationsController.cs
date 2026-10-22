using Microsoft.AspNetCore.Mvc;
using MonitoringApi.Application;
using MonitoringApi.Contracts.Requests;
using MonitoringApi.Contracts.Responses;

using MonitoringApi.Entities;

namespace MonitoringApi.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(NotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<Notification>>> List(
        [FromQuery] NotificationQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Notification>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));
}

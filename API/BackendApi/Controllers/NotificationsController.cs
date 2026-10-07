using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;
using BackendApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<NotificationResponse>>> List(
        [FromQuery] NotificationQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));
}

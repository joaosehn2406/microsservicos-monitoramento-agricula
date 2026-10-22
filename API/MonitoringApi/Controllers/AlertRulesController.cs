using Microsoft.AspNetCore.Mvc;
using MonitoringApi.Application;
using MonitoringApi.Contracts.Requests;

using MonitoringApi.Entities;

namespace MonitoringApi.Controllers;

[ApiController]
[Route("api/rules")]
public sealed class AlertRulesController(AlertRuleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertRule>>> List(
        [FromQuery] AlertRuleQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlertRule>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<AlertRule>> Create(
        CreateAlertRuleRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AlertRule>> Update(
        Guid id,
        UpdateAlertRuleRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

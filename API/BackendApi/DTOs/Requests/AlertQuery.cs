namespace BackendApi.DTOs.Requests;

/// <summary><c>from</c>/<c>to</c> filter on created_at.</summary>
public sealed class AlertQuery : PageQuery
{
    public Guid? PropertyId { get; init; }
    public string? Variable { get; init; }
    public string? Severity { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}

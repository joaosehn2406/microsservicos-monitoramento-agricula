namespace BackendApi.DTOs.Requests;

/// <summary><c>from</c>/<c>to</c> filter on observed_at.</summary>
public sealed class ReadingQuery : PageQuery
{
    public Guid? PropertyId { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}

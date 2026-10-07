using Analytics.Entities;

namespace Analytics.Repositories;

public interface IAlertRepository
{
    /// <summary>
    /// Inserts the processed-event marker and the alerts in one transaction (SPEC 3.3).
    /// Returns false when the event was already processed.
    /// </summary>
    Task<bool> TryAddAsync(
        ProcessedEvent processedEvent,
        IReadOnlyList<Alert> alerts,
        CancellationToken cancellationToken);
}

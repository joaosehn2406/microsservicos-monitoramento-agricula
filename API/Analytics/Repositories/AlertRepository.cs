using Analytics.Data;
using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Analytics.Repositories;

public sealed class AlertRepository(AnalyticsDbContext context) : IAlertRepository
{
    private static readonly HashSet<string> DuplicateConstraints =
        ["pk_processed_events", "uq_alerts_source_event_id_rule_id"];

    public async Task<bool> TryAddAsync(
        ProcessedEvent processedEvent,
        IReadOnlyList<Alert> alerts,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            context.ProcessedEvents.Add(processedEvent);
            await context.SaveChangesAsync(cancellationToken);

            if (alerts.Count > 0)
            {
                context.Alerts.AddRange(alerts);
                await context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                                                   {
                                                       SqlState: PostgresErrorCodes.UniqueViolation
                                                   } postgres
                                                   && DuplicateConstraints.Contains(postgres.ConstraintName ?? ""))
        {
            await transaction.RollbackAsync(CancellationToken.None);
            context.ChangeTracker.Clear();
            return false;
        }
    }
}

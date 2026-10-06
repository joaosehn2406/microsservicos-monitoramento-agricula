using Microsoft.EntityFrameworkCore;
using Npgsql;
using Storage.Data;
using Storage.Entities;

namespace Storage.Repositories;

public sealed class WeatherReadingRepository(StorageDbContext context) : IWeatherReadingRepository
{
    private static readonly HashSet<string> DuplicateConstraints =
        ["pk_processed_events", "uq_weather_readings_event_id"];

    public async Task<bool> TryAddAsync(
        ProcessedEvent processedEvent,
        WeatherReading reading,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            context.ProcessedEvents.Add(processedEvent);
            await context.SaveChangesAsync(cancellationToken);

            context.WeatherReadings.Add(reading);
            await context.SaveChangesAsync(cancellationToken);

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

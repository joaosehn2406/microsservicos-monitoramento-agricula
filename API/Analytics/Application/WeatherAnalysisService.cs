using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Analytics.Infrastructure.Messaging;
using Analytics.Infrastructure.Persistence;
using Analytics.Contracts.Events;
using Analytics.Domain;

namespace Analytics.Application;

public sealed class WeatherAnalysisService(
    AnalyticsDbContext context,
    AlertPublisher publisher,
    ILogger<WeatherAnalysisService> logger)
{
    public const string ConsumerName = "analytics";
    private static readonly HashSet<string> DuplicateConstraints =
        ["pk_processed_events", "uq_alerts_source_event_id_rule_id"];

    public async Task<ProcessingResult> ProcessAsync(WeatherReadingEvent message, CancellationToken cancellationToken)
    {
        Validate(message);

        var rules = await context.AlertRules.AsNoTracking()
            .Where(rule => rule.PropertyId == message.PropertyId && rule.IsActive)
            .ToListAsync(cancellationToken);
        var alerts = Evaluate(message, rules);

        var processedEvent = new ProcessedEvent { EventId = message.EventId, Consumer = ConsumerName };
        try
        {
            context.ProcessedEvents.Add(processedEvent);
            context.Alerts.AddRange(alerts);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                                                   {
                                                       SqlState: PostgresErrorCodes.UniqueViolation
                                                   } postgres
                                                   && DuplicateConstraints.Contains(postgres.ConstraintName ?? ""))
        {
            context.ChangeTracker.Clear();
            return ProcessingResult.Duplicate;
        }

        var published = 0;
        foreach (var alert in alerts)
        {
            if (await publisher.PublishAsync(ToEvent(alert), CancellationToken.None))
            {
                published++;
            }
            else
            {
                logger.LogError(
                    "Alert {AlertId} of reading {EventId} is stored but was not published; no notification will be sent",
                    alert.Id, message.EventId);
            }
        }

        logger.LogInformation(
            "Reading {EventId} analysed: {RulesEvaluated} rules evaluated, {AlertsRaised} alerts raised, " +
            "{AlertsPublished} published",
            message.EventId, rules.Count, alerts.Count, published);

        return ProcessingResult.Processed;
    }

    private List<Alert> Evaluate(WeatherReadingEvent message, IReadOnlyList<AlertRule> rules)
    {
        var alerts = new List<Alert>();
        foreach (var rule in rules)
        {
            if (!AlertRuleEvaluator.TryGetMeasuredValue(rule.Variable, message, out var measuredValue))
            {
                logger.LogWarning("Rule {RuleId} has unknown variable {Variable}; ignored (event {EventId})",
                    rule.Id, rule.Variable, message.EventId);
                continue;
            }

            if (!AlertRuleEvaluator.TryCompare(measuredValue, rule.Operator, rule.Threshold, out var violated))
            {
                logger.LogWarning("Rule {RuleId} has unknown operator {Operator}; ignored (event {EventId})",
                    rule.Id, rule.Operator, message.EventId);
                continue;
            }

            if (violated)
            {
                alerts.Add(new Alert
                {
                    EventId = Guid.CreateVersion7(),
                    SourceEventId = message.EventId,
                    RuleId = rule.Id,
                    PropertyId = message.PropertyId,
                    PropertyName = message.PropertyName,
                    Variable = rule.Variable,
                    Operator = rule.Operator,
                    Threshold = rule.Threshold,
                    MeasuredValue = measuredValue,
                    Severity = rule.Severity,
                    ObservedAt = message.ObservedAt.ToUniversalTime()
                });
            }
        }

        return alerts;
    }

    private static AlertRaisedEvent ToEvent(Alert alert)
    {
        var (label, unit) = alert.Variable switch
        {
            "temperature" => ("temperatura", "°C"),
            "humidity" => ("umidade", "%"),
            "precipitation" => ("precipitação", "mm"),
            "soil_moisture" => ("umidade do solo", "m³/m³"),
            _ => (alert.Variable, "")
        };
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var measured = alert.MeasuredValue.ToString("0.###", culture);
        var threshold = alert.Threshold.ToString("0.###", culture);
        var observedAt = alert.ObservedAt.ToUniversalTime()
            .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        return new AlertRaisedEvent
        {
            EventId = alert.EventId,
            AlertId = alert.Id,
            PropertyId = alert.PropertyId,
            Severity = alert.Severity,
            Title = $"Alerta {alert.Severity}: {label} — {alert.PropertyName}",
            Message = $"{char.ToUpperInvariant(label[0])}{label[1..]} medida de {measured} {unit} " +
                      $"(regra: {alert.Operator} {threshold} {unit}) em {observedAt} UTC."
        };
    }

    private static void Validate(WeatherReadingEvent message)
    {
        var errors = new List<string>();

        if (message.EventId == Guid.Empty) errors.Add("eventId is empty");
        if (string.IsNullOrWhiteSpace(message.ClientId)) errors.Add("clientId is empty");
        if (message.PropertyId == Guid.Empty) errors.Add("propertyId is empty");
        if (string.IsNullOrWhiteSpace(message.PropertyName)) errors.Add("propertyName is empty");
        if (message.ObservedAt.Kind == DateTimeKind.Unspecified) errors.Add("observedAt has no UTC offset");
        if (!InRange(message.Latitude, -90, 90)) errors.Add("latitude is out of range");
        if (!InRange(message.Longitude, -180, 180)) errors.Add("longitude is out of range");
        if (!double.IsFinite(message.TemperatureCelsius)) errors.Add("temperatureCelsius is not a number");
        if (!InRange(message.RelativeHumidityPercent, 0, 100)) errors.Add("relativeHumidityPercent is out of range");
        if (!InRange(message.PrecipitationMm, 0, double.MaxValue)) errors.Add("precipitationMm is out of range");
        if (!InRange(message.SoilMoisture, 0, 1)) errors.Add("soilMoisture is out of range");

        if (errors.Count > 0)
        {
            throw new InvalidMessageException($"Invalid weather.reading event: {string.Join("; ", errors)}");
        }
    }

    private static bool InRange(double value, double min, double max) =>
        double.IsFinite(value) && value >= min && value <= max;
}

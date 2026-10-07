using Analytics.Entities;
using Analytics.Exceptions;
using Analytics.Mappers;
using Analytics.Messaging.Contracts;
using Analytics.Messaging.Infrastructure;
using Analytics.Repositories;

namespace Analytics.Services;

public sealed class WeatherAnalysisService(
    IAlertRuleRepository ruleRepository,
    IAlertRepository alertRepository,
    ILogger<WeatherAnalysisService> logger) : IWeatherAnalysisService
{
    public const string ConsumerName = "analytics";
    public const string ExpectedEventType = "weather.reading";

    public async Task<ProcessingResult> ProcessAsync(WeatherReadingEvent message, CancellationToken cancellationToken)
    {
        Validate(message);

        var rules = await ruleRepository.GetActiveByPropertyAsync(message.PropertyId, cancellationToken);
        var alerts = Evaluate(message, rules);

        var processedEvent = new ProcessedEvent { EventId = message.EventId, Consumer = ConsumerName };
        var added = await alertRepository.TryAddAsync(processedEvent, alerts, cancellationToken);
        if (!added)
        {
            return ProcessingResult.Duplicate;
        }

        logger.LogInformation(
            "Reading {EventId} analysed: {RulesEvaluated} rules evaluated, {AlertsRaised} alerts raised",
            message.EventId, rules.Count, alerts.Count);

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
                alerts.Add(AlertMapper.ToEntity(message, rule, measuredValue));
            }
        }

        return alerts;
    }

    private static void Validate(WeatherReadingEvent message)
    {
        var errors = new List<string>();

        if (message.EventId == Guid.Empty) errors.Add("eventId is empty");
        if (message.EventType != ExpectedEventType) errors.Add($"eventType must be '{ExpectedEventType}'");
        if (string.IsNullOrWhiteSpace(message.Source)) errors.Add("source is empty");
        if (string.IsNullOrWhiteSpace(message.ClientId)) errors.Add("clientId is empty");
        if (message.PropertyId == Guid.Empty) errors.Add("propertyId is empty");
        if (string.IsNullOrWhiteSpace(message.PropertyName)) errors.Add("propertyName is empty");
        if (message.OccurredAt.Kind == DateTimeKind.Unspecified) errors.Add("occurredAt has no UTC offset");
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

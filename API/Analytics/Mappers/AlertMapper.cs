using Analytics.Entities;
using Analytics.Messaging.Contracts;

namespace Analytics.Mappers;

public static class AlertMapper
{
    public static Alert ToEntity(WeatherReadingEvent reading, AlertRule rule, double measuredValue) => new()
    {
        Id = Guid.CreateVersion7(),
        EventId = Guid.CreateVersion7(),
        SourceEventId = reading.EventId,
        RuleId = rule.Id,
        PropertyId = reading.PropertyId,
        PropertyName = reading.PropertyName,
        Variable = rule.Variable,
        Operator = rule.Operator,
        Threshold = rule.Threshold,
        MeasuredValue = measuredValue,
        Severity = rule.Severity,
        ObservedAt = reading.ObservedAt.ToUniversalTime()
    };
}

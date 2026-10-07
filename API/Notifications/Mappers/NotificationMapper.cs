using System.Globalization;
using Notifications.Entities;
using Notifications.Messaging.Contracts;

namespace Notifications.Mappers;

public static class NotificationMapper
{
    private static readonly Dictionary<string, (string Label, string Unit)> Variables = new()
    {
        ["temperature"] = ("temperatura", "°C"),
        ["humidity"] = ("umidade", "%"),
        ["precipitation"] = ("precipitação", "mm"),
        ["soil_moisture"] = ("umidade do solo", "m³/m³")
    };

    /// <summary>Comma decimal separator built from the invariant culture, so no ICU culture data is needed.</summary>
    private static readonly NumberFormatInfo DecimalComma = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = "."
    };

    public static bool IsKnownVariable(string variable) => Variables.ContainsKey(variable);

    /// <summary>The variable must be known (see <see cref="IsKnownVariable"/>).</summary>
    public static Notification ToEntity(AlertRaisedEvent message) => new()
    {
        Id = Guid.CreateVersion7(),
        AlertId = message.AlertId,
        AlertEventId = message.EventId,
        PropertyId = message.PropertyId,
        Severity = message.Severity,
        Title = BuildTitle(message),
        Message = BuildMessage(message)
    };

    // "Alerta high: temperatura — Fazenda Blumenau"
    private static string BuildTitle(AlertRaisedEvent message) =>
        $"Alerta {message.Severity}: {Variables[message.Variable].Label} — {message.PropertyName}";

    // "Temperatura medida de 31,4 °C (regra: > 30 °C) em 06/10/2026 12:00 UTC."
    private static string BuildMessage(AlertRaisedEvent message)
    {
        var (label, unit) = Variables[message.Variable];
        var observedAt = message.ObservedAt.ToUniversalTime()
            .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        return $"{char.ToUpperInvariant(label[0])}{label[1..]} medida de {FormatNumber(message.MeasuredValue)} {unit} " +
               $"(regra: {message.Operator} {FormatNumber(message.Threshold)} {unit}) em {observedAt} UTC.";
    }

    private static string FormatNumber(double value) => value.ToString("0.###", DecimalComma);
}

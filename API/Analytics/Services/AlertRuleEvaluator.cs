using Analytics.Messaging.Contracts;

namespace Analytics.Services;

public static class AlertRuleEvaluator
{
    /// <summary>Maps a rule variable to the reading field. False for an unknown variable.</summary>
    public static bool TryGetMeasuredValue(string variable, WeatherReadingEvent reading, out double value)
    {
        double? measured = variable switch
        {
            "temperature" => reading.TemperatureCelsius,
            "humidity" => reading.RelativeHumidityPercent,
            "precipitation" => reading.PrecipitationMm,
            "soil_moisture" => reading.SoilMoisture,
            _ => null
        };

        value = measured ?? 0;
        return measured.HasValue;
    }

    /// <summary>Applies <c>value operator threshold</c>. False for an unknown operator.</summary>
    public static bool TryCompare(double value, string @operator, double threshold, out bool violated)
    {
        bool? result = @operator switch
        {
            ">" => value > threshold,
            "<" => value < threshold,
            ">=" => value >= threshold,
            "<=" => value <= threshold,
            _ => null
        };

        violated = result ?? false;
        return result.HasValue;
    }
}

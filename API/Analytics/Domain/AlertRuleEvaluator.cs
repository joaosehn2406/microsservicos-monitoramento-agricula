using Analytics.Contracts.Events;

namespace Analytics.Domain;

public static class AlertRuleEvaluator
{
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

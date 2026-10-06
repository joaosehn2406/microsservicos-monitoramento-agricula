using System.Globalization;
using WeatherCollector.Configurations;
using WeatherCollector.DTOs.OpenMeteo;
using WeatherCollector.Exceptions;
using WeatherCollector.Messaging.Contracts;

namespace WeatherCollector.Mappers;

public static class WeatherReadingMapper
{
    public static WeatherReadingEvent ToEvent(
        OpenMeteoResponse response,
        FarmClient client,
        FarmProperty property,
        DateTimeOffset collectedAt)
    {
        var current = response.Current
            ?? throw new OpenMeteoException("A resposta da Open-Meteo não contém dados atuais.");

        if (!DateTimeOffset.TryParse(
                current.Time,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var observedAt))
        {
            throw new OpenMeteoException("O horário atual retornado pela Open-Meteo é inválido.");
        }

        return new WeatherReadingEvent(
            Guid.NewGuid(),
            collectedAt.ToUniversalTime(),
            client.Id,
            property.Id,
            property.Name,
            property.Latitude,
            property.Longitude,
            observedAt,
            current.TemperatureCelsius
                ?? throw new OpenMeteoException("Temperatura ausente na resposta."),
            current.RelativeHumidityPercent
                ?? throw new OpenMeteoException("Umidade relativa ausente na resposta."),
            current.PrecipitationMillimeters
                ?? throw new OpenMeteoException("Precipitação ausente na resposta."),
            current.SoilMoisture
                ?? throw new OpenMeteoException("Umidade do solo ausente na resposta."));
    }
}

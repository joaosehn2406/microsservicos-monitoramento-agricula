using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using WeatherCollector.Configurations;
using WeatherCollector.DTOs.OpenMeteo;
using WeatherCollector.Exceptions;

namespace WeatherCollector.Clients;

public sealed class OpenMeteoClient(HttpClient httpClient) : IOpenMeteoClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<OpenMeteoResponse> GetCurrentAsync(
        FarmProperty property,
        CancellationToken cancellationToken)
    {
        var latitude = property.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = property.Longitude.ToString(CultureInfo.InvariantCulture);
        const string variables =
            "temperature_2m,relative_humidity_2m,precipitation,soil_moisture_0_to_1cm";

        var requestUri =
            $"/v1/forecast?latitude={latitude}&longitude={longitude}" +
            $"&current={variables}&timezone=UTC&timeformat=iso8601";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new OpenMeteoException("A Open-Meteo respondeu com HTTP 429 (limite de requisições).");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new OpenMeteoException(
                $"A Open-Meteo respondeu com HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        var result = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(
            cancellationToken: cancellationToken);

        if (result?.Current is not
            {
                Time: not null,
                TemperatureCelsius: not null,
                RelativeHumidityPercent: not null,
                PrecipitationMillimeters: not null,
                SoilMoisture: not null
            })
        {
            throw new OpenMeteoException("A resposta da Open-Meteo está incompleta.");
        }

        return result;
    }
}

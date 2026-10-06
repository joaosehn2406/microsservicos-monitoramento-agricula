namespace WeatherCollector.Clients;

public sealed class OpenMeteoClient(HttpClient httpClient) : IOpenMeteoClient
{
    private readonly HttpClient _httpClient = httpClient;
}

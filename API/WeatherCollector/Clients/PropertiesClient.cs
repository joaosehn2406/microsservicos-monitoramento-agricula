namespace WeatherCollector.Clients;

public sealed class PropertiesClient(HttpClient httpClient) : IPropertiesClient
{
    private readonly HttpClient _httpClient = httpClient;
}

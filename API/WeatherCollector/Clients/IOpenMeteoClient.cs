using WeatherCollector.Configurations;
using WeatherCollector.DTOs.OpenMeteo;

namespace WeatherCollector.Clients;

public interface IOpenMeteoClient
{
    Task<OpenMeteoResponse> GetCurrentAsync(
        FarmProperty property,
        CancellationToken cancellationToken);
}

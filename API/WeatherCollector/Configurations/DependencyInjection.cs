using WeatherCollector.Clients;
using WeatherCollector.Messaging.Publishers;
using WeatherCollector.Services;
using WeatherCollector.Workers;

namespace WeatherCollector.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IWeatherCollectorService, WeatherCollectorService>();
        services.AddSingleton<IWeatherPublisher, WeatherPublisher>();
        services.AddHttpClient<IPropertiesClient, PropertiesClient>(client =>
            client.BaseAddress = new Uri(configuration["Services:Properties:BaseUrl"]!))
            .AddStandardResilienceHandler();
        services.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>(client =>
            client.BaseAddress = new Uri(configuration["Services:OpenMeteo:BaseUrl"]!))
            .AddStandardResilienceHandler();
        services.AddHostedService<WeatherCollectionWorker>();

        return services;
    }
}

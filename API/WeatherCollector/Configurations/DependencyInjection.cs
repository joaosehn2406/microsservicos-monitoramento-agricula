using Microsoft.EntityFrameworkCore;
using WeatherCollector.Clients;
using WeatherCollector.Data;
using WeatherCollector.Exceptions;
using WeatherCollector.Messaging.Publishers;
using WeatherCollector.Repositories;
using WeatherCollector.Services;
using WeatherCollector.Workers;

namespace WeatherCollector.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<WeatherCollectorDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<ICollectionExecutionRepository, CollectionExecutionRepository>();
        services.AddScoped<IWeatherCollectorService, WeatherCollectorService>();
        services.AddSingleton<IWeatherPublisher, WeatherPublisher>();
        services.AddHttpClient<IPropertiesClient, PropertiesClient>(client =>
            client.BaseAddress = new Uri(configuration["Services:Properties:BaseUrl"]!))
            .AddStandardResilienceHandler();
        services.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>(client =>
            client.BaseAddress = new Uri(configuration["Services:OpenMeteo:BaseUrl"]!))
            .AddStandardResilienceHandler();
        services.AddHostedService<WeatherCollectionWorker>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

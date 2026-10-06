using WeatherCollector.Clients;
using WeatherCollector.Messaging.Publishers;
using WeatherCollector.Workers;

namespace WeatherCollector.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CollectorOptions>(
            configuration.GetSection(CollectorOptions.SectionName));
        services.Configure<OpenMeteoOptions>(
            configuration.GetSection(OpenMeteoOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<CollectorLimitsStartupValidator>();
        services.AddSingleton<IHostedService>(serviceProvider =>
            serviceProvider.GetRequiredService<CollectorLimitsStartupValidator>());

        services.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>(client =>
        {
            var options = configuration
                .GetSection(OpenMeteoOptions.SectionName)
                .Get<OpenMeteoOptions>() ?? new OpenMeteoOptions();

            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddSingleton<WeatherPublisher>();
        services.AddSingleton<IWeatherPublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<WeatherPublisher>());
        services.AddSingleton<IHostedService>(serviceProvider =>
            serviceProvider.GetRequiredService<WeatherPublisher>());

        services.AddSingleton(TimeProvider.System);

        for (var index = 0; index < FarmCatalog.Clients.Count; index++)
        {
            var clientIndex = index;
            services.AddSingleton<IHostedService>(serviceProvider =>
                ActivatorUtilities.CreateInstance<ClientCollectorJob>(
                    serviceProvider,
                    FarmCatalog.Clients[clientIndex],
                    clientIndex));
        }

        return services;
    }
}

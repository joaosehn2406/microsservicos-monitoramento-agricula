using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Messaging.Consumers;
using Storage.Messaging.Infrastructure;
using Storage.Repositories;
using Storage.Services;
using Storage.Workers;

namespace Storage.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StorageDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<DlqReprocessOptions>(configuration.GetSection(DlqReprocessOptions.SectionName));
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddScoped<IWeatherReadingRepository, WeatherReadingRepository>();
        services.AddScoped<IWeatherReadingService, WeatherReadingService>();
        services.AddHostedService<WeatherReadingConsumer>();
        services.AddHostedService<DlqReprocessingJob>();

        return services;
    }
}

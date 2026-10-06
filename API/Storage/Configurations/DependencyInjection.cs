using Microsoft.EntityFrameworkCore;
using Storage.Data;
using Storage.Exceptions;
using Storage.Messaging.Consumers;
using Storage.Repositories;
using Storage.Services;

namespace Storage.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StorageDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IWeatherReadingRepository, WeatherReadingRepository>();
        services.AddScoped<IWeatherReadingService, WeatherReadingService>();
        services.AddSingleton<WeatherReadingConsumer>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using BackendApi.Data;
using BackendApi.Exceptions;
using BackendApi.Repositories;
using BackendApi.Services;

namespace BackendApi.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BackendDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<IAlertRuleService, AlertRuleService>();
        services.AddScoped<IWeatherReadingRepository, WeatherReadingRepository>();
        services.AddScoped<IWeatherReadingService, WeatherReadingService>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddOpenApi();

        return services;
    }
}

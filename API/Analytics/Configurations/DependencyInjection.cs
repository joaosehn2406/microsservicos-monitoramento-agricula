using Analytics.Data;
using Analytics.Messaging.Consumers;
using Analytics.Messaging.Publishers;
using Analytics.Repositories;
using Analytics.Services;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IAlertRuleService, AlertRuleService>();
        services.AddScoped<IWeatherAnalysisService, WeatherAnalysisService>();
        services.AddSingleton<WeatherReadingConsumer>();
        services.AddSingleton<IAlertPublisher, AlertPublisher>();

        return services;
    }
}

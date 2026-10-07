using Analytics.Data;
using Analytics.Messaging.Consumers;
using Analytics.Messaging.Infrastructure;
using Analytics.Messaging.Publishers;
using Analytics.Repositories;
using Analytics.Services;
using Analytics.Workers;
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
        services.Configure<DlqReprocessOptions>(configuration.GetSection(DlqReprocessOptions.SectionName));
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<IAlertPublisher, AlertPublisher>();
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IWeatherAnalysisService, WeatherAnalysisService>();
        services.AddHostedService<WeatherReadingConsumer>();
        services.AddHostedService<DlqReprocessingJob>();

        return services;
    }
}

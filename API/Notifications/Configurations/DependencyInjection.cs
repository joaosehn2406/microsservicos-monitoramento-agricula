using Microsoft.EntityFrameworkCore;
using Notifications.Data;
using Notifications.Messaging.Consumers;
using Notifications.Messaging.Infrastructure;
using Notifications.Repositories;
using Notifications.Services;
using Notifications.Workers;

namespace Notifications.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<DlqReprocessOptions>(configuration.GetSection(DlqReprocessOptions.SectionName));
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddHostedService<AlertRaisedConsumer>();
        services.AddHostedService<DlqReprocessingJob>();

        return services;
    }
}

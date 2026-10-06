using Microsoft.EntityFrameworkCore;
using Notifications.Data;
using Notifications.Messaging.Consumers;
using Notifications.Repositories;
using Notifications.Services;

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
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<AlertCreatedConsumer>();

        return services;
    }
}

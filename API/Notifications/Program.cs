using Notifications.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Notifications.Application;
using Notifications.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<AlertRaisedConsumer>();

var host = builder.Build();
host.Run();

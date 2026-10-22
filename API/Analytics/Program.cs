using Analytics.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Analytics.Application;
using Analytics.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddSingleton<AlertPublisher>();
builder.Services.AddScoped<WeatherAnalysisService>();
builder.Services.AddHostedService<WeatherReadingConsumer>();

var host = builder.Build();
host.Run();

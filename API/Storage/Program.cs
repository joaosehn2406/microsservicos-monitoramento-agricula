using Storage.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Storage.Application;
using Storage.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<StorageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddScoped<WeatherReadingService>();
builder.Services.AddHostedService<WeatherReadingConsumer>();

var host = builder.Build();
host.Run();

using WeatherCollector.Infrastructure.Clients;
using WeatherCollector.Infrastructure.Messaging;
using WeatherCollector.Infrastructure.Options;
using WeatherCollector.Infrastructure.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK ";
});
builder.Services.Configure<CollectorOptions>(builder.Configuration.GetSection(CollectorOptions.SectionName));
builder.Services.Configure<OpenMeteoOptions>(builder.Configuration.GetSection(OpenMeteoOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddSingleton<CollectorLimitsStartupValidator>();
builder.Services.AddSingleton<IHostedService>(services =>
    services.GetRequiredService<CollectorLimitsStartupValidator>());

builder.Services.AddHttpClient<OpenMeteoClient>(client =>
{
    var options = builder.Configuration
        .GetSection(OpenMeteoOptions.SectionName)
        .Get<OpenMeteoOptions>() ?? new OpenMeteoOptions();

    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddSingleton<WeatherPublisher>();
builder.Services.AddSingleton<IHostedService>(services => services.GetRequiredService<WeatherPublisher>());
builder.Services.AddSingleton(TimeProvider.System);

for (var index = 0; index < FarmCatalog.Clients.Count; index++)
{
    var clientIndex = index;
    builder.Services.AddSingleton<IHostedService>(services =>
        ActivatorUtilities.CreateInstance<ClientCollectorJob>(
            services, FarmCatalog.Clients[clientIndex], clientIndex));
}

var host = builder.Build();
await host.RunAsync();

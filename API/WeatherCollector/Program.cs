using WeatherCollector.Configurations;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK ";
});
builder.Services.AddApplicationServices(builder.Configuration);

var host = builder.Build();
await host.RunAsync();

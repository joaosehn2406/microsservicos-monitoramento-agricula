using Properties.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.Run();

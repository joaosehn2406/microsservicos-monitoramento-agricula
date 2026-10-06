using Microsoft.EntityFrameworkCore;
using Properties.Data;
using Properties.Exceptions;
using Properties.Repositories;
using Properties.Services;

namespace Properties.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PropertiesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

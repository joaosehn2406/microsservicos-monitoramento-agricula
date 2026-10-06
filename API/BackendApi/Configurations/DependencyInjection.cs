using Microsoft.EntityFrameworkCore;
using BackendApi.Data;
using BackendApi.Exceptions;
using BackendApi.Repositories;
using BackendApi.Services;

namespace BackendApi.Configurations;

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

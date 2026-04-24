using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using FadeAfro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FadeAfro.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<DatabaseContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMasterProfileRepository, MasterProfileRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IMasterScheduleRepository, MasterScheduleRepository>();
        services.AddScoped<IMasterUnavailabilityRepository, MasterUnavailabilityRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}

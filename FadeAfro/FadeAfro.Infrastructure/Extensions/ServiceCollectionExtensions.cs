using System.Text;
using FadeAfro.Application.Services;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Repositories;
using FadeAfro.Domain.Services;
using FadeAfro.Infrastructure.HealthChecks;
using FadeAfro.Infrastructure.Options;
using FadeAfro.Infrastructure.Options.Workers;
using FadeAfro.Infrastructure.Persistence;
using FadeAfro.Infrastructure.Repositories;
using FadeAfro.Infrastructure.Services;
using FadeAfro.Infrastructure.Settings;
using FadeAfro.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;

namespace FadeAfro.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((jwtBearerOptions, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
                };
            });

        return services;
    }

    public static IServiceCollection AddTelegram(this IServiceCollection services)
    {
        services.AddOptions<TelegramOptions>()
            .BindConfiguration("Telegram")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITelegramSettings, TelegramSettings>();
        
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
            return new TelegramBotClient(options.BotToken);
        });

        return services;
    }

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
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
    
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    public static IServiceCollection AddTimeSettings(this IServiceCollection services)
    {
        services.AddOptions<TimeOptions>()
            .BindConfiguration("Time")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ITimeSettings, TimeSettings>();

        return services;
    }

    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddOptions<AppointmentReminderWorkerOptions>()
            .BindConfiguration("Workers:AppointmentReminderWorker")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<AppointmentReminderWorker>();

        return services;
    }

    public static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }

    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");

        return services;
    }
}

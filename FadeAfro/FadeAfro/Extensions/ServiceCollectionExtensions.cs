using System.Text.Json.Serialization;

namespace FadeAfro.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddControllersWithOptions(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.InferSecuritySchemes();
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (environment.IsDevelopment())
        {
            // В dev — секция обязательна
            if (allowedOrigins is null || allowedOrigins.Length == 0)
                throw new InvalidOperationException("Cors:AllowedOrigins is not configured.");

            if (allowedOrigins.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Cors:AllowedOrigins contains null or empty values.");
        }
        else
        {
            // В prod — секция опциональна (если фронт и бэк на одном домене, CORS не нужен)
            if (allowedOrigins is null || allowedOrigins.Length == 0)
                return services;

            if (allowedOrigins.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Cors:AllowedOrigins contains null or empty values.");
        }

        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedOrigins!)
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

        return services;
    }
}

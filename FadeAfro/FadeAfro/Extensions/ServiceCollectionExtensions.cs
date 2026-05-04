using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FadeAfro.Constants;

namespace FadeAfro.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        static RateLimitPartition<string> ByIp(HttpContext ctx, int permitLimit) =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });

        static RateLimitPartition<string> ByUser(HttpContext ctx, int permitLimit) =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anon",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // ── Anonymous (ключ: IP) ───────────────────────────────────────────

            options.AddPolicy(RateLimitingPolicies.AuthLogin, ctx =>
                ByIp(ctx, permitLimit: 10));

            options.AddPolicy(RateLimitingPolicies.MasterPhoto, ctx =>
                ByIp(ctx, permitLimit: 20));

            options.AddPolicy(RateLimitingPolicies.MasterAvailability, ctx =>
                ByIp(ctx, permitLimit: 20));

            // ── Authenticated write — medium priority (ключ: userId) ──────────

            options.AddPolicy(RateLimitingPolicies.Booking, ctx =>
                ByUser(ctx, permitLimit: 5));

            options.AddPolicy(RateLimitingPolicies.PhotoUpload, ctx =>
                ByUser(ctx, permitLimit: 7));

            options.AddPolicy(RateLimitingPolicies.OwnerAssignMaster, ctx =>
                ByUser(ctx, permitLimit: 5));

            options.AddPolicy(RateLimitingPolicies.OwnerDismissMaster, ctx =>
                ByUser(ctx, permitLimit: 5));

            // ── Authenticated write — low priority (ключ: userId) ────────────

            options.AddPolicy(RateLimitingPolicies.AppointmentCancelClient, ctx =>
                ByUser(ctx, permitLimit: 10));

            options.AddPolicy(RateLimitingPolicies.AppointmentCancelMaster, ctx =>
                ByUser(ctx, permitLimit: 10));

            options.AddPolicy(RateLimitingPolicies.ScheduleSet, ctx =>
                ByUser(ctx, permitLimit: 30));

            options.AddPolicy(RateLimitingPolicies.ScheduleDelete, ctx =>
                ByUser(ctx, permitLimit: 30));

            options.AddPolicy(RateLimitingPolicies.UnavailabilityAdd, ctx =>
                ByUser(ctx, permitLimit: 20));

            options.AddPolicy(RateLimitingPolicies.UnavailabilityDelete, ctx =>
                ByUser(ctx, permitLimit: 20));

            options.AddPolicy(RateLimitingPolicies.ServiceAdd, ctx =>
                ByUser(ctx, permitLimit: 20));

            options.AddPolicy(RateLimitingPolicies.ServiceUpdate, ctx =>
                ByUser(ctx, permitLimit: 20));

            options.AddPolicy(RateLimitingPolicies.ServiceDelete, ctx =>
                ByUser(ctx, permitLimit: 20));
        });

        return services;
    }

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

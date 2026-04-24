using Microsoft.Extensions.DependencyInjection;

namespace FadeAfro.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => options.EnableAnnotations());

        return services;
    }
}

using Microsoft.Extensions.Hosting;
using Serilog;

namespace FadeAfro.Infrastructure.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration));
    }
}

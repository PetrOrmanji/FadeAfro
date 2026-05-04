using FadeAfro.Middleware;
using Serilog;

namespace FadeAfro.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSwaggerWithUi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static WebApplication UseExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }

    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseSerilogRequestLogging();

        return app;
    }
}

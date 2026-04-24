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
}

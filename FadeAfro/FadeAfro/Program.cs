using System.Reflection;
using FadeAfro.Application.Extensions;
using FadeAfro.Extensions;
using FadeAfro.Infrastructure.Extensions;
using ServiceCollectionExtensions = FadeAfro.Infrastructure.Extensions.ServiceCollectionExtensions;

SetCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilog();

builder.Services.AddApplication();
builder.Services.AddPostgres(builder.Configuration);
builder.Services.AddJwt();
builder.Services.AddTelegram();
builder.Services.AddTimeSettings();
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);
builder.Services.AddNotifications();
builder.Services.AddFileStorage();
builder.Services.AddControllersWithOptions();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiting();
builder.Services.AddSwagger();
builder.Services.AddHealthCheckServices();

var app = builder.Build();

app.UseRequestLogging();
app.UseExceptionHandling();
app.UseSwaggerWithUi();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void SetCurrentDirectory()
{
    var entryAssembly = Assembly.GetEntryAssembly();
    if (entryAssembly is null)
    {
        return;
    }

    var assemblyDirectory = Path.GetDirectoryName(entryAssembly.Location);
    if (assemblyDirectory is null)
    {
        return;
    }

    Directory.SetCurrentDirectory(assemblyDirectory);
}

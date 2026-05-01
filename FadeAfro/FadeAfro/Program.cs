using System.Reflection;
using FadeAfro.Application.Extensions;
using FadeAfro.Extensions;
using FadeAfro.Infrastructure.Extensions;

SetCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPostgres(builder.Configuration);
builder.Services.AddJwt();
builder.Services.AddTelegram();
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);
builder.Services.AddNotifications();
builder.Services.AddFileStorage();
builder.Services.AddControllersWithOptions();
builder.Services.AddAuthorization();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseExceptionHandling();
app.UseSwaggerWithUi();
app.UseCors();
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

using FadeAfro.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPostgres(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

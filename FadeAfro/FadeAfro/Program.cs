using FadeAfro.Application.Extensions;
using FadeAfro.Extensions;
using FadeAfro.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPostgres(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseSwaggerWithUi();

app.MapControllers();

app.Run();

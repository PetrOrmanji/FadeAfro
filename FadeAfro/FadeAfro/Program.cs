using FadeAfro.Application.Extensions;
using FadeAfro.Extensions;
using FadeAfro.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPostgres(builder.Configuration);
builder.Services.AddJwt();
builder.Services.AddTelegram();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseExceptionHandling();
app.UseSwaggerWithUi();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

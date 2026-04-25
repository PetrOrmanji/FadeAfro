FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FadeAfro.sln .
COPY FadeAfro/FadeAfro.csproj FadeAfro/
COPY FadeAfro.Application/FadeAfro.Application.csproj FadeAfro.Application/
COPY FadeAfro.Domain/FadeAfro.Domain.csproj FadeAfro.Domain/
COPY FadeAfro.Infrastructure/FadeAfro.Infrastructure.csproj FadeAfro.Infrastructure/
COPY FadeAfro.Tests/FadeAfro.Tests.csproj FadeAfro.Tests/

RUN dotnet restore

COPY . .

RUN dotnet publish FadeAfro/FadeAfro.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FadeAfro.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FadeAfro.sln .
COPY FadeAfro/FadeAfro.csproj FadeAfro/
COPY FadeAfro.Application/FadeAfro.Application.csproj FadeAfro.Application/
COPY FadeAfro.Domain/FadeAfro.Domain.csproj FadeAfro.Domain/
COPY FadeAfro.Infrastructure/FadeAfro.Infrastructure.csproj FadeAfro.Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet publish FadeAfro/FadeAfro.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --no-create-home appuser

COPY --from=build /app/publish .

RUN mkdir -p logs && chown appuser logs

USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "FadeAfro.dll"]

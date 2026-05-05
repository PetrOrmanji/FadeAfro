using FadeAfro.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FadeAfro.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DatabaseContext _dbContext;

    public DatabaseHealthCheck(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Unable to connect to the database.");
    }
}

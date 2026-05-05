using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("health")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Health check", Description = "Returns the health status of the application and its dependencies.")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var result = report.Entries.ToDictionary(
            e => e.Key,
            e => e.Value.Status.ToString());

        return report.Status == HealthStatus.Healthy
            ? Ok(result)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}

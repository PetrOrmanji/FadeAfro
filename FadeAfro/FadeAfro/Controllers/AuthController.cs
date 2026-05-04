using FadeAfro.Application.Features.Auth.AuthenticateTelegramUser;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingPolicies.AuthLogin)]
    [SwaggerOperation(
        Summary = "Authenticate via Telegram", 
        Description = "Validates Telegram Mini App initData and returns a JWT token.")]
    public async Task<IActionResult> Login([FromBody] AuthenticateTelegramUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}

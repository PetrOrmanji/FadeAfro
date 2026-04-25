using FadeAfro.Application.Features.Users.GetUser;
using FadeAfro.Application.Features.Users.RegisterUser;
using FadeAfro.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/users")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register a new user", Description = "Creates a new user by Telegram ID.")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByTelegramId), new { telegramId = command.TelegramId }, response);
    }

    [HttpGet("get/{telegramId}")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get user by Telegram ID", Description = "Returns a user with the given Telegram ID.")]
    public async Task<IActionResult> GetByTelegramId(long telegramId)
    {
        var response = await _mediator.Send(new GetUserQuery(telegramId));
        return Ok(response);
    }
}

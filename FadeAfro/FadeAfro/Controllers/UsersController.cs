using System.Security.Claims;
using FadeAfro.Application.Features.Users.GetAllUsers;
using FadeAfro.Application.Features.Users.GetUser;
using FadeAfro.Application.Features.Users.RegisterUser;
using FadeAfro.Application.Features.Users.UpdateUserName;
using UpdateUserNameRequest = FadeAfro.Application.Features.Users.UpdateUserName.UpdateUserNameRequest;
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

    [HttpPut("update-name")]
    [Authorize]
    [SwaggerOperation(Summary = "Update user's display name", Description = "Updates the first and last name of the currently authenticated user.")]
    public async Task<IActionResult> UpdateName([FromBody] UpdateUserNameRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _mediator.Send(new UpdateUserNameCommand(userId, request.FirstName, request.LastName));
        return NoContent();
    }

    [HttpGet("all")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get all users (paged)")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(page, pageSize, search));
        return Ok(result);
    }
}

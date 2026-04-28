using System.Security.Claims;
using FadeAfro.Application.Features.Users.GetAllUsers;
using FadeAfro.Application.Features.Users.GetCurrentUser;
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

    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Get current user info", Description = "Returns the currently authenticated user's name.")]
    public async Task<IActionResult> GetMe()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _mediator.Send(new GetCurrentUserQuery(userId));
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

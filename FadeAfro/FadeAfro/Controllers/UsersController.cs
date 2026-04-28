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
        var userId = GetUserId();
        var getUserQuery = new GetUserQuery(userId);
        
        var response = await _mediator.Send(getUserQuery);
        return Ok(response);
    }
    
    [HttpGet("all")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get all users (paged)")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersRequest request)
    {
        var getAllUsersQuery = new GetAllUsersQuery(
            request.Page,
            request.PageSize,
            request.Search);
        
        var result = await _mediator.Send(getAllUsersQuery);
        return Ok(result);
    }

    [HttpPut("update-full-name")]
    [Authorize]
    [SwaggerOperation(Summary = "Update user's full name", Description = "Updates the first and last name of the currently authenticated user.")]
    public async Task<IActionResult> UpdateName([FromBody] UpdateUserNameRequest request)
    {
        var userId = GetUserId();
        var updateUserFullNameCommand = new UpdateUserFullNameCommand(userId, request.FirstName, request.LastName);
        
        await _mediator.Send(updateUserFullNameCommand);
        return NoContent();
    }
    
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

using FadeAfro.Application.Features.Users.GetAllUsers;
using FadeAfro.Application.Features.Users.GetUser;
using FadeAfro.Application.Features.Users.UpdateUserFullName;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
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
    
    [HttpGet("get-all-users")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get all users (paged)")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("get-current-user")]
    [Authorize]
    [SwaggerOperation(Summary = "Get current user info")]
    public async Task<IActionResult> GetMe() 
    {
        var getUserQuery = new GetUserQuery(User.GetUserId());
        
        var response = await _mediator.Send(getUserQuery);
        return Ok(response);
    }
    
    [HttpPost("update-current-user-full-name")]
    [Authorize]
    [SwaggerOperation(Summary = "Update the first and last name of the user")]
    public async Task<IActionResult> UpdateName([FromBody] UpdateUserNameRequest request)
    {
        var updateUserFullNameCommand = new UpdateUserFullNameCommand(
            User.GetUserId(), 
            request.FirstName, 
            request.LastName);
        
        await _mediator.Send(updateUserFullNameCommand);
        return NoContent();
    }
}

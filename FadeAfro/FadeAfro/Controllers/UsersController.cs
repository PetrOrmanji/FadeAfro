using FadeAfro.Application.Features.Users.GetMasters;
using FadeAfro.Application.Features.Users.GetOwners;
using FadeAfro.Application.Features.Users.GetUser;
using FadeAfro.Application.Features.Users.GetUsers;
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
    
    [HttpGet("get/all")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get all users (paged)")]
    public async Task<IActionResult> GetAll([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("get/masters")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get all masters")]
    public async Task<IActionResult> GetMasters()
    {
        var getMastersQuery = new GetMastersQuery();
        
        var result = await _mediator.Send(getMastersQuery);
        return Ok(result);
    }
    
    [HttpGet("get/owners")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Get all owners")]
    public async Task<IActionResult> GetOwners()
    {
        var getOwnersQuery = new GetOwnersQuery();
        
        var result = await _mediator.Send(getOwnersQuery);
        return Ok(result);
    }

    [HttpGet("get/me")]
    [Authorize]
    [SwaggerOperation(Summary = "Get my user info")]
    public async Task<IActionResult> GetMe() 
    {
        var getUserQuery = new GetUserQuery(User.GetUserId());
        
        var response = await _mediator.Send(getUserQuery);
        return Ok(response);
    }
    
    [HttpPut("update/me/full-name")]
    [Authorize]
    [SwaggerOperation(Summary = "Update my first and last name")]
    public async Task<IActionResult> UpdateMyFullName([FromBody] UpdateUserFullNameRequest request)
    {
        var updateUserFullNameCommand = new UpdateUserFullNameCommand(
            User.GetUserId(), 
            request.FirstName, 
            request.LastName);
        
        await _mediator.Send(updateUserFullNameCommand);
        return NoContent();
    }
}

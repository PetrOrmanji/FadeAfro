using FadeAfro.Application.Features.MasterUnavailabilities.AddMasterUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.DeleteMasteUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;
using FadeAfro.Domain.Constants;
using FadeAfro.Constants;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-unavailabilities")]
[Tags("Master Unavailabilities")]
[Authorize]
public class MasterUnavailabilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterUnavailabilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get master unavailabilities")]
    public async Task<IActionResult> GetMasterUnavailabilities(Guid masterProfileId)
    {
        var getMasterUnavailabilitiesQuery = new GetMasterUnavailabilitiesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterUnavailabilitiesQuery);
        return Ok(response);
    }

    [HttpPost("add/me")]
    [Authorize(Roles = Roles.Master)]
    [EnableRateLimiting(RateLimitingPolicies.UnavailabilityAdd)]
    [SwaggerOperation(Summary = "Add my unavailability date")]
    public async Task<IActionResult> AddMyUnavailability([FromBody] AddMasterUnavailabilityRequest request)
    {
        var addUnavailabilityCommand = new AddMasterUnavailabilityCommand(
            User.GetUserId(),
            request.Date);
        
        await _mediator.Send(addUnavailabilityCommand);
        return Ok();
    }

    [HttpDelete("delete/me/{unavailabilityId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [EnableRateLimiting(RateLimitingPolicies.UnavailabilityDelete)]
    [SwaggerOperation(Summary = "Delete my unavailability date")]
    public async Task<IActionResult> DeleteMyUnavailability(Guid unavailabilityId)
    {
        var deleteUnavailabilityCommand = new DeleteMasterUnavailabilityCommand(
            User.GetUserId(),
            unavailabilityId);
        
        await _mediator.Send(deleteUnavailabilityCommand);
        return NoContent();
    }
}

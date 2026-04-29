using FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [SwaggerOperation(
        Summary = "Get master's unavailabilities by master profile ID", 
        Description = "Returns all unavailability entries for the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterUnavailabilitiesQuery = new GetMasterUnavailabilitiesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterUnavailabilitiesQuery);
        return Ok(response);
    }

    [HttpPost("add")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(
        Summary = "Add master's unavailability", 
        Description = "Marks a date (or time range within a date) as unavailable for the master profile.")]
    public async Task<IActionResult> Add([FromBody] AddUnavailabilityRequest request)
    {
        var addUnavailabilityCommand = new AddUnavailabilityCommand(
            User.GetUserId(),
            request.Date);
        
        await _mediator.Send(addUnavailabilityCommand);
        return Ok();
    }

    [HttpDelete("delete/{unavailabilityId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(
        Summary = "Delete master's unavailability", 
        Description = "Deletes the unavailability entry with the given ID.")]
    public async Task<IActionResult> Delete(Guid unavailabilityId)
    {
        var deleteUnavailabilityCommand = new DeleteUnavailabilityCommand(
            User.GetUserId(),
            unavailabilityId);
        
        await _mediator.Send(deleteUnavailabilityCommand);
        return NoContent();
    }
}

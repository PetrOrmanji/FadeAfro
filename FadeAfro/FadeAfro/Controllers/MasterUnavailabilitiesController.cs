using FadeAfro.Application.Features.MasterUnavailabilities.AddMasterUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.DeleteMasteUnavailability;
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
    
    [HttpGet("get-master-unavailabilities/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get master's unavailabilities.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterUnavailabilitiesQuery = new GetMasterUnavailabilitiesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterUnavailabilitiesQuery);
        return Ok(response);
    }

    [HttpPost("add-master-unavailability")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Add master's unavailability")]
    public async Task<IActionResult> Add([FromBody] AddMasterUnavailabilityRequest request)
    {
        var addUnavailabilityCommand = new AddMasterUnavailabilityCommand(
            User.GetUserId(),
            request.Date);
        
        await _mediator.Send(addUnavailabilityCommand);
        return Ok();
    }

    [HttpDelete("delete-master-unavailability/{unavailabilityId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Delete master's unavailability")]
    public async Task<IActionResult> Delete(Guid unavailabilityId)
    {
        var deleteUnavailabilityCommand = new DeleteMasterUnavailabilityCommand(
            User.GetUserId(),
            unavailabilityId);
        
        await _mediator.Send(deleteUnavailabilityCommand);
        return NoContent();
    }
}

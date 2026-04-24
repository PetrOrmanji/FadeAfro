using FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;
using FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-unavailabilities")]
[Tags("Master Unavailabilities")]
public class MasterUnavailabilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterUnavailabilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    [SwaggerOperation(Summary = "Add an unavailability", Description = "Marks a date (or time range within a date) as unavailable for the master profile.")]
    public async Task<IActionResult> Add([FromBody] AddUnavailabilityCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByMasterProfileId), new { masterProfileId = command.MasterProfileId }, response);
    }

    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get unavailabilities by master profile ID", Description = "Returns all unavailability entries for the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var response = await _mediator.Send(new GetMasterUnavailabilitiesQuery(masterProfileId));
        return Ok(response);
    }

    [HttpDelete("delete/{unavailabilityId:guid}")]
    [SwaggerOperation(Summary = "Delete an unavailability", Description = "Deletes the unavailability entry with the given ID.")]
    public async Task<IActionResult> Delete(Guid unavailabilityId)
    {
        await _mediator.Send(new DeleteUnavailabilityCommand(unavailabilityId));
        return NoContent();
    }
}

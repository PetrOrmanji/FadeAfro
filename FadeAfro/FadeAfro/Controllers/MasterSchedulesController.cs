using FadeAfro.Application.Features.MasterSchedules.DeleteSchedule;
using FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;
using FadeAfro.Application.Features.MasterSchedules.SetSchedule;
using FadeAfro.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-schedules")]
[Tags("Master Schedules")]
[Authorize]
public class MasterSchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterSchedulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("set")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Set a schedule", Description = "Sets a working schedule for a specific day of week for the master profile.")]
    public async Task<IActionResult> Set([FromBody] SetScheduleCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByMasterProfileId), new { masterProfileId = command.MasterProfileId }, response);
    }

    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get schedule by master profile ID", Description = "Returns all schedule entries for the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var response = await _mediator.Send(new GetMasterScheduleQuery(masterProfileId));
        return Ok(response);
    }

    [HttpDelete("delete/{scheduleId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Delete a schedule entry", Description = "Deletes the schedule entry with the given ID.")]
    public async Task<IActionResult> Delete(Guid scheduleId)
    {
        await _mediator.Send(new DeleteScheduleCommand(scheduleId));
        return NoContent();
    }
}

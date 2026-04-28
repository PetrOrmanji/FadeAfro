using FadeAfro.Application.Features.MasterSchedules.DeleteSchedule;
using FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;
using FadeAfro.Application.Features.MasterSchedules.SetSchedule;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
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
    
    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get schedule by master profile ID", Description = "Returns all schedule entries for the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterScheduleQuery = new GetMasterScheduleQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterScheduleQuery);
        return Ok(response);
    }

    [HttpPost("set")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Set master's schedule", Description = "Sets a working schedule for a specific day of week for the master profile.")]
    public async Task<IActionResult> Set([FromBody] SetScheduleRequest request)
    {
        var setScheduleCommand = new SetScheduleCommand(
            User.GetUserId(),
            request.DayOfWeek,
            request.StartTime,
            request.EndTime);
        
        await _mediator.Send(setScheduleCommand);
        return Ok();
    }

    [HttpDelete("delete/{scheduleId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Delete master's schedule entry", Description = "Deletes master's schedule entry with the given ID.")]
    public async Task<IActionResult> Delete(Guid scheduleId)
    {
        var deleteScheduleCommand = new DeleteScheduleCommand(
            User.GetUserId(),
            scheduleId);
        
        await _mediator.Send(deleteScheduleCommand);
        
        return NoContent();
    }
}

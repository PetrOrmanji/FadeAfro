using FadeAfro.Application.Features.MasterSchedules.DeleteMasterSchedule;
using FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;
using FadeAfro.Application.Features.MasterSchedules.SetMasterSchedule;
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
    [SwaggerOperation(Summary = "Get master's schedule")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterScheduleQuery = new GetMasterScheduleQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterScheduleQuery);
        return Ok(response);
    }

    [HttpPost("set/me")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Set my master profile schedule")]
    public async Task<IActionResult> Set([FromBody] SetMasterScheduleRequest request)
    {
        var setScheduleCommand = new SetMasterScheduleCommand(
            User.GetUserId(),
            request.DayOfWeek,
            request.StartTime,
            request.EndTime);
        
        await _mediator.Send(setScheduleCommand);
        return Ok();
    }

    [HttpDelete("delete/me/{scheduleId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Delete my master profile schedule")]
    public async Task<IActionResult> Delete(Guid scheduleId)
    {
        var deleteScheduleCommand = new DeleteMasterScheduleCommand(
            User.GetUserId(),
            scheduleId);
        
        await _mediator.Send(deleteScheduleCommand);
        
        return NoContent();
    }
}

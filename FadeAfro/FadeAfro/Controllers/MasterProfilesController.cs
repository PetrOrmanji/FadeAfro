using FadeAfro.Application.Features.MasterProfiles.CreateMasterProfile;
using FadeAfro.Application.Features.MasterProfiles.DismissMaster;
using FadeAfro.Application.Features.MasterProfiles.GetAllMasters;
using FadeAfro.Application.Features.MasterProfiles.GetAvailableSlots;
using FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;
using FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfile;
using FadeAfro.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-profiles")]
[Tags("Master Profiles")]
[Authorize]
public class MasterProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Create a master profile", Description = "Creates a new master profile for an existing user with the Master role.")]
    public async Task<IActionResult> Create([FromBody] CreateMasterProfileCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { masterProfileId = response.Id }, response);
    }

    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get master profile by ID", Description = "Returns a master profile with the given ID.")]
    public async Task<IActionResult> GetById(Guid masterProfileId)
    {
        var response = await _mediator.Send(new GetMasterProfileQuery(masterProfileId));
        return Ok(response);
    }

    [HttpGet("all")]
    [SwaggerOperation(Summary = "Get all master profiles", Description = "Returns a list of all master profiles.")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllMastersQuery());
        return Ok(response);
    }

    [HttpPut("update/{masterProfileId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Update master profile", Description = "Updates photo URL and description of the master profile.")]
    public async Task<IActionResult> Update(Guid masterProfileId, [FromBody] UpdateMasterProfileCommand command)
    {
        await _mediator.Send(command with { MasterProfileId = masterProfileId });
        return NoContent();
    }

    [HttpDelete("dismiss/{userId:guid}")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(Summary = "Dismiss master", Description = "Revokes the Master role and deletes the master profile.")]
    public async Task<IActionResult> Dismiss(Guid userId)
    {
        await _mediator.Send(new DismissMasterCommand(userId));
        return NoContent();
    }

    [HttpGet("available-slots/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get available slots", Description = "Returns available time slots for booking with the given master on the specified date.")]
    public async Task<IActionResult> GetAvailableSlots(Guid masterProfileId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date)
    {
        var response = await _mediator.Send(new GetAvailableSlotsQuery(masterProfileId, serviceId, date));
        return Ok(response);
    }
}

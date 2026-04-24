using FadeAfro.Application.Features.MasterProfiles.CreateMasterProfile;
using FadeAfro.Application.Features.MasterProfiles.GetAllMasters;
using FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;
using FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-profiles")]
[Tags("Master Profiles")]
public class MasterProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
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
    [SwaggerOperation(Summary = "Update master profile", Description = "Updates photo URL and description of the master profile.")]
    public async Task<IActionResult> Update(Guid masterProfileId, [FromBody] UpdateMasterProfileCommand command)
    {
        await _mediator.Send(command with { MasterProfileId = masterProfileId });
        return NoContent();
    }
}

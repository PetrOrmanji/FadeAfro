using FadeAfro.Application.Features.Services.AddService;
using FadeAfro.Application.Features.Services.DeleteService;
using FadeAfro.Application.Features.Services.GetMasterServices;
using FadeAfro.Application.Features.Services.UpdateService;
using FadeAfro.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/services")]
[Tags("Services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Add a service", Description = "Adds a new service to the master profile.")]
    public async Task<IActionResult> Add([FromBody] AddServiceCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByMasterProfileId), new { masterProfileId = command.MasterProfileId }, response);
    }

    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get services by master profile ID", Description = "Returns all services of the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var response = await _mediator.Send(new GetMasterServicesQuery(masterProfileId));
        return Ok(response);
    }

    [HttpPut("update/{serviceId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Update a service", Description = "Updates name, description, price and duration of the service.")]
    public async Task<IActionResult> Update(Guid serviceId, [FromBody] UpdateServiceCommand command)
    {
        await _mediator.Send(command with { ServiceId = serviceId });
        return NoContent();
    }

    [HttpDelete("delete/{serviceId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Delete a service", Description = "Deletes the service with the given ID.")]
    public async Task<IActionResult> Delete(Guid serviceId)
    {
        await _mediator.Send(new DeleteServiceCommand(serviceId));
        return NoContent();
    }
}

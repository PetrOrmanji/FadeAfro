using FadeAfro.Application.Features.Services.AddService;
using FadeAfro.Application.Features.Services.DeleteService;
using FadeAfro.Application.Features.Services.GetMasterServices;
using FadeAfro.Application.Features.Services.UpdateService;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
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
    
    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(
        Summary = "Get services by master profile ID", 
        Description = "Returns all services of the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterServicesQuery = new GetMasterServicesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterServicesQuery);
        return Ok(response);
    }

    [HttpPost("add")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Add a service",
        Description = "Adds a new service to the master profile.")]
    public async Task<IActionResult> Add([FromBody] AddServiceRequest request)
    {
        var addServiceCommand = new AddServiceCommand(
            User.GetUserId(),
            request.Name,
            request.Description,
            request.Price,
            request.Duration);
        
        await _mediator.Send(addServiceCommand);
        return Ok();
    }

    [HttpPut("update/{serviceId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Update a master's service", 
        Description = "Updates name, description, price and duration of the master's service.")]
    public async Task<IActionResult> Update(Guid serviceId, [FromBody] UpdateServiceRequest request)
    {
        var updateServiceCommand = new UpdateServiceCommand(
            User.GetUserId(),
            serviceId,
            request.Name,
            request.Description,
            request.Price,
            request.Duration);
        
        await _mediator.Send(updateServiceCommand);
        return NoContent();
    }

    [HttpDelete("delete/{serviceId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Delete a master's service",
        Description = "Deletes the master's service with the given ID.")]
    public async Task<IActionResult> Delete(Guid serviceId)
    {
        var deleteServiceCommand = new DeleteServiceCommand(
            User.GetUserId(),
            serviceId);
        
        await _mediator.Send(deleteServiceCommand);
        return NoContent();
    }
}

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
    
    [HttpGet("get-master-services/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get master's services")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterServicesQuery = new GetMasterServicesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterServicesQuery);
        return Ok(response);
    }

    [HttpPost("add-master-service")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Add a new master's service")]
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

    [HttpPut("update-master-service/{serviceId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Update master's service")]
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

    [HttpDelete("delete-master-service/{serviceId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Delete master's service")]
    public async Task<IActionResult> Delete(Guid serviceId)
    {
        var deleteServiceCommand = new DeleteServiceCommand(
            User.GetUserId(),
            serviceId);
        
        await _mediator.Send(deleteServiceCommand);
        return NoContent();
    }
}

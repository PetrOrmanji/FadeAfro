using FadeAfro.Application.Features.MasterServices.AddMasterService;
using FadeAfro.Application.Features.MasterServices.DeleteMasterService;
using FadeAfro.Application.Features.MasterServices.GetMasterServices;
using FadeAfro.Application.Features.MasterServices.UpdateMasterService;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-services")]
[Tags("Services")]
[Authorize]
public class MasterServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get master's services")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var getMasterServicesQuery = new GetMasterServicesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterServicesQuery);
        return Ok(response);
    }

    [HttpPost("add")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Add master's service")]
    public async Task<IActionResult> Add([FromBody] AddMasterServiceRequest request)
    {
        var addServiceCommand = new AddMasterServiceCommand(
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
    [SwaggerOperation(Summary = "Update master's service")]
    public async Task<IActionResult> Update(Guid serviceId, [FromBody] UpdateMasterServiceRequest request)
    {
        var updateServiceCommand = new UpdateMasterServiceCommand(
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
    [SwaggerOperation(Summary = "Delete master's service")]
    public async Task<IActionResult> Delete(Guid serviceId)
    {
        var deleteServiceCommand = new DeleteMasterServiceCommand(
            User.GetUserId(),
            serviceId);
        
        await _mediator.Send(deleteServiceCommand);
        return NoContent();
    }
}

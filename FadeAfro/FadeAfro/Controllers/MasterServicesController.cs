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
    public async Task<IActionResult> GetMasterServices(Guid masterProfileId)
    {
        var getMasterServicesQuery = new GetMasterServicesQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterServicesQuery);
        return Ok(response);
    }

    [HttpPost("add/me")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Add my master profile service")]
    public async Task<IActionResult> AddMyService([FromBody] AddMasterServiceRequest request)
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

    [HttpPut("update/me/{serviceId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Update my master profile service")]
    public async Task<IActionResult> UpdateMyService(Guid serviceId, [FromBody] UpdateMasterServiceRequest request)
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

    [HttpDelete("delete/me/{serviceId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Delete my master profile service")]
    public async Task<IActionResult> DeleteMyService(Guid serviceId)
    {
        var deleteServiceCommand = new DeleteMasterServiceCommand(
            User.GetUserId(),
            serviceId);
        
        await _mediator.Send(deleteServiceCommand);
        return NoContent();
    }
}

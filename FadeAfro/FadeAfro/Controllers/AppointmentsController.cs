using FadeAfro.Application.Features.Appointments.CancelAppointmentByClient;
using FadeAfro.Application.Features.Appointments.CancelAppointmentByMaster;
using FadeAfro.Application.Features.Appointments.CreateAppointment;
using FadeAfro.Application.Features.Appointments.GetClientActualAppointments;
using FadeAfro.Application.Features.Appointments.GetMasterActualAppointments;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/appointments")]
[Tags("Appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("get-actual-client-appointments")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(
        Summary = "Get actual client appointments", 
        Description = "Returns actual client appointments.")]
    public async Task<IActionResult> GetActualClientAppointments()
    {
        var getClientActualAppointmentsQuery = new GetClientActualAppointmentsQuery(User.GetUserId());
        
        var response = await _mediator.Send(getClientActualAppointmentsQuery);
        return Ok(response);
    }

    [HttpGet("get-actual-master-appointments")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Get actual master appointments", 
        Description = "Returns actual master appointments.")]
    public async Task<IActionResult> GetActualMasterAppointments()
    {
        var getMasterActualAppointmentsQuery = new GetMasterActualAppointmentsQuery(User.GetUserId());
        
        var response = await _mediator.Send(getMasterActualAppointmentsQuery);
        return Ok(response);
    }

    [HttpPost("create")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(
        Summary = "Create an appointment", 
        Description = "Books a new appointment for the client with the specified master and service.")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        var createdAppointmentCommand = new CreateAppointmentCommand(
            User.GetUserId(),
            request.MasterProfileId,
            request.ServiceIds,
            request.StartTime,
            request.Comment);
        
        await _mediator.Send(createdAppointmentCommand);
        return Ok();
    }
    
    [HttpPatch("cancel-by-client/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(
        Summary = "Cancel an appointment by client", 
        Description = "Cancels the appointment on behalf of the client.")]
    public async Task<IActionResult> CancelByClient(Guid appointmentId)
    {
        var cancelAppointmentByClientCommand = new CancelAppointmentByClientCommand(
            User.GetUserId(),
            appointmentId);
        
        await _mediator.Send(cancelAppointmentByClientCommand);
        return NoContent();
    }

    [HttpPatch("cancel-by-master/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Cancel an appointment by master", 
        Description = "Cancels the appointment on behalf of the master.")]
    public async Task<IActionResult> CancelByMaster(Guid appointmentId)
    {
        var cancelAppointmentByClientCommand = new CancelAppointmentByMasterCommand(
            User.GetUserId(),
            appointmentId);
        
        await _mediator.Send(cancelAppointmentByClientCommand);
        return NoContent();
    }
}

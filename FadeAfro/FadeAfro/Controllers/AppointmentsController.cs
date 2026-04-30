using FadeAfro.Application.Features.Appointments.CancelAppointmentByClient;
using FadeAfro.Application.Features.Appointments.CancelAppointmentByMaster;
using FadeAfro.Application.Features.Appointments.CreateClientAppointment;
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
    
    [HttpGet("client/get/me/appointments")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(
        Summary = "Get my actual client appointments", 
        Description = "Returns my actual client appointments.")]
    public async Task<IActionResult> GetMyActualClientAppointments()
    {
        var getClientActualAppointmentsQuery = new GetClientActualAppointmentsQuery(User.GetUserId());
        
        var response = await _mediator.Send(getClientActualAppointmentsQuery);
        return Ok(response);
    }

    [HttpGet("master/get/me/appointments")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Get my actual master appointments", 
        Description = "Returns my actual master appointments.")]
    public async Task<IActionResult> GetMyActualMasterAppointments()
    {
        var getMasterActualAppointmentsQuery = new GetMasterActualAppointmentsQuery(User.GetUserId());
        
        var response = await _mediator.Send(getMasterActualAppointmentsQuery);
        return Ok(response);
    }

    [HttpPost("client/me/book")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(
        Summary = "Create an appointment", 
        Description = "Books a new appointment for the client with the specified master and service.")]
    public async Task<IActionResult> Book([FromBody] CreateClientAppointmentRequest request)
    {
        var createdAppointmentCommand = new CreateClientAppointmentCommand(
            User.GetUserId(),
            request.MasterProfileId,
            request.ServiceIds,
            request.StartTime,
            request.Comment);
        
        await _mediator.Send(createdAppointmentCommand);
        return Ok();
    }
    
    [HttpPatch("client/cancel/me/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(
        Summary = "Cancel an appointment by client", 
        Description = "Cancels the appointment on behalf of the client.")]
    public async Task<IActionResult> CancelMyAppointmentByClient(Guid appointmentId)
    {
        var cancelAppointmentByClientCommand = new CancelAppointmentByClientCommand(
            User.GetUserId(),
            appointmentId);
        
        await _mediator.Send(cancelAppointmentByClientCommand);
        return NoContent();
    }

    [HttpPatch("master/cancel/me/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Cancel an appointment by master", 
        Description = "Cancels the appointment on behalf of the master.")]
    public async Task<IActionResult> CancelByAppointmentByMaster(Guid appointmentId)
    {
        var cancelAppointmentByClientCommand = new CancelAppointmentByMasterCommand(
            User.GetUserId(),
            appointmentId);
        
        await _mediator.Send(cancelAppointmentByClientCommand);
        return NoContent();
    }
}

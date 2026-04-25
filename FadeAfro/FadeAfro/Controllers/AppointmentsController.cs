using FadeAfro.Application.Features.Appointments.CancelAppointment;
using FadeAfro.Application.Features.Appointments.CompleteAppointment;
using FadeAfro.Application.Features.Appointments.CreateAppointment;
using FadeAfro.Application.Features.Appointments.GetClientAppointments;
using FadeAfro.Application.Features.Appointments.GetMasterAppointments;
using FadeAfro.Domain.Constants;
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

    [HttpPost("create")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(Summary = "Create an appointment", Description = "Books a new appointment for the client with the specified master and service.")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByClientId), new { clientId = command.ClientId }, response);
    }

    [HttpGet("get/client/{clientId:guid}")]
    [Authorize(Roles = Roles.ClientOrOwner)]
    [SwaggerOperation(Summary = "Get appointments by client ID", Description = "Returns all appointments for the given client.")]
    public async Task<IActionResult> GetByClientId(Guid clientId)
    {
        var response = await _mediator.Send(new GetClientAppointmentsQuery(clientId));
        return Ok(response);
    }

    [HttpGet("get/master/{masterProfileId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Get appointments by master profile ID", Description = "Returns all appointments for the given master profile.")]
    public async Task<IActionResult> GetByMasterProfileId(Guid masterProfileId)
    {
        var response = await _mediator.Send(new GetMasterAppointmentsQuery(masterProfileId));
        return Ok(response);
    }

    [HttpPatch("cancel-by-client/{appointmentId:guid}")]
    [Authorize(Roles = Roles.Client)]
    [SwaggerOperation(Summary = "Cancel an appointment by client", Description = "Cancels the appointment on behalf of the client.")]
    public async Task<IActionResult> CancelByClient(Guid appointmentId)
    {
        await _mediator.Send(new CancelAppointmentCommand(appointmentId, CancelledByMaster: false));
        return NoContent();
    }

    [HttpPatch("cancel-by-master/{appointmentId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Cancel an appointment by master", Description = "Cancels the appointment on behalf of the master.")]
    public async Task<IActionResult> CancelByMaster(Guid appointmentId)
    {
        await _mediator.Send(new CancelAppointmentCommand(appointmentId, CancelledByMaster: true));
        return NoContent();
    }

    [HttpPatch("complete/{appointmentId:guid}")]
    [Authorize(Roles = Roles.MasterOrOwner)]
    [SwaggerOperation(Summary = "Complete an appointment", Description = "Marks the appointment as completed.")]
    public async Task<IActionResult> Complete(Guid appointmentId)
    {
        await _mediator.Send(new CompleteAppointmentCommand(appointmentId));
        return NoContent();
    }
}

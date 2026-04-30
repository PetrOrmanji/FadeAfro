using MediatR;

namespace FadeAfro.Application.Features.Appointments.CreateClientAppointment;

public record CreateClientAppointmentCommand(
    Guid ClientId,
    Guid MasterProfileId,
    List<Guid> ServiceIds,
    DateTime StartTime,
    string? Comment) : IRequest;

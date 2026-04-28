using MediatR;

namespace FadeAfro.Application.Features.Appointments.CreateAppointment;

public record CreateAppointmentCommand(
    Guid ClientId,
    Guid MasterProfileId,
    List<Guid> ServiceIds,
    DateTime StartTime,
    string? Comment) : IRequest<CreateAppointmentResponse>;

using MediatR;

namespace FadeAfro.Application.Features.Appointments.CreateAppointment;

public record CreateAppointmentCommand(
    Guid ClientId,
    Guid MasterProfileId,
    Guid ServiceId,
    DateTime StartTime,
    string? Comment) : IRequest<CreateAppointmentResponse>;

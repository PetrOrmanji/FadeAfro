using MediatR;

namespace FadeAfro.Application.Features.Appointments.CompleteAppointment;

public record CompleteAppointmentCommand(Guid AppointmentId) : IRequest<Unit>;

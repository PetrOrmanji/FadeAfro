using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, bool CancelledByMaster) : IRequest<Unit>;

using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByClient;

public record CancelAppointmentByClientCommand(Guid ClientId, Guid AppointmentId) : IRequest;

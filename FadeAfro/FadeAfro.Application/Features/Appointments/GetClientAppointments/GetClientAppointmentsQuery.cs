using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public record GetClientAppointmentsQuery(Guid ClientId) : IRequest<GetClientAppointmentsResponse>;

using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientActiveAppointments;

public record GetClientActiveAppointmentsQuery(Guid ClientId) : IRequest<GetClientActiveAppointmentsResponse>;

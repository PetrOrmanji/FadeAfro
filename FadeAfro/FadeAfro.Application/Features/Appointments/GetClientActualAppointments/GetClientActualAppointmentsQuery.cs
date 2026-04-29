using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientActualAppointments;

public record GetClientActualAppointmentsQuery(Guid ClientId) : IRequest<GetClientActualAppointmentsResponse>;

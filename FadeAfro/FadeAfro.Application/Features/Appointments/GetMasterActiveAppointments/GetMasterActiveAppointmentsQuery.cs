using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterActiveAppointments;

public record GetMasterActiveAppointmentsQuery(Guid MasterId) : IRequest<GetMasterActiveAppointmentsResponse>;

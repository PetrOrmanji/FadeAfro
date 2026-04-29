using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterActualAppointments;

public record GetMasterActualAppointmentsQuery(Guid MasterId) : IRequest<GetMasterActualAppointmentsResponse>;

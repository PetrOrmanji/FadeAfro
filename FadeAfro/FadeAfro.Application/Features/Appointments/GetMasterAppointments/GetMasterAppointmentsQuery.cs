using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public record GetMasterAppointmentsQuery(Guid MasterProfileId) : IRequest<GetMasterAppointmentsResponse>;

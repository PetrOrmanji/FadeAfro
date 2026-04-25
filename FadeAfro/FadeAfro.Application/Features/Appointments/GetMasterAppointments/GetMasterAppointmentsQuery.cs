using FadeAfro.Application.Common;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public record GetMasterAppointmentsQuery(Guid MasterProfileId, PaginationParams Pagination) : IRequest<PagedResponse<MasterAppointmentResponse>>;

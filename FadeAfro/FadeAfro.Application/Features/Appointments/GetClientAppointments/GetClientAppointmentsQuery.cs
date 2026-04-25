using FadeAfro.Application.Common;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public record GetClientAppointmentsQuery(Guid ClientId, PaginationParams Pagination) : IRequest<PagedResponse<AppointmentResponse>>;

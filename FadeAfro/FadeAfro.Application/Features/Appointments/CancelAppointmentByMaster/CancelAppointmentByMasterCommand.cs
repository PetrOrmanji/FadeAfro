using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByMaster;

public record CancelAppointmentByMasterCommand(
    Guid MasterId,
    Guid AppointmentId) : IRequest;
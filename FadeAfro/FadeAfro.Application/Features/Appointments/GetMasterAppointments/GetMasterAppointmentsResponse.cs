using FadeAfro.Application.Common.Appointments;
using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public record GetMasterAppointmentsResponse(
    Guid Id,
    Guid ClientId,
    IReadOnlyList<AppointmentServiceDto> Services,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Comment);

using FadeAfro.Application.Common.Appointments;
using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public record GetClientAppointmentsResponse(
    Guid Id,
    Guid MasterProfileId,
    string MasterName,
    string? MasterPhotoUrl,
    IReadOnlyList<AppointmentServiceDto> Services,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Comment);

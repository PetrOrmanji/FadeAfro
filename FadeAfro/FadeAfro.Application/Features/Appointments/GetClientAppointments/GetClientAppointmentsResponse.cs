using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public record AppointmentResponse(
    Guid Id,
    Guid MasterProfileId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Comment);

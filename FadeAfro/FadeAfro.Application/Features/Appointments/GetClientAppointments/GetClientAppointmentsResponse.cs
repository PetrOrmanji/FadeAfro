using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public record AppointmentResponse(
    Guid Id,
    Guid MasterProfileId,
    string MasterName,
    string? MasterPhotoUrl,
    Guid ServiceId,
    string ServiceName,
    decimal ServicePrice,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Comment);

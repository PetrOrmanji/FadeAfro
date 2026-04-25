using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public record MasterAppointmentResponse(
    Guid Id,
    Guid ClientId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Comment);

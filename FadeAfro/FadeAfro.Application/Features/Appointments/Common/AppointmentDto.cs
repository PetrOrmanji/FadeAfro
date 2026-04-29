namespace FadeAfro.Application.Features.Appointments.Common;

public record AppointmentDto(
    Guid Id,
    DateTime StartTime,
    DateTime EndTime,
    string? Comment,
    List<AppointmentServiceDto> Services,
    AppointmentMasterDto? Master = null,
    AppointmentClientDto? Client = null);
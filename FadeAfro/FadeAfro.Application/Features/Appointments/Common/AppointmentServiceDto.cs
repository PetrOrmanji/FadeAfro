namespace FadeAfro.Application.Features.Appointments.Common;

public record AppointmentServiceDto(
    Guid? ServiceId,
    string ServiceName,
    int Price,
    TimeSpan Duration);
namespace FadeAfro.Application.Common.Appointments;

public record AppointmentServiceDto(
    Guid? ServiceId,
    string ServiceName,
    int Price,
    TimeSpan Duration);
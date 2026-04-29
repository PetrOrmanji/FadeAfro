namespace FadeAfro.Application.Features.Appointments.Common;

public record AppointmentClientDto(
    string FirstName,
    string? LastName,
    string? UserName);
namespace FadeAfro.Application.Features.Appointments.Common;

public record AppointmentMasterDto(
    string FirstName,
    string? LastName,
    string? Description,
    string? PhotoUrl);
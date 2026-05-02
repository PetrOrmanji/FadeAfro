namespace FadeAfro.Application.Features.Appointments.Common;

public record AppointmentMasterDto(
    Guid MasterProfileId,
    string FirstName,
    string? LastName,
    string? Description);
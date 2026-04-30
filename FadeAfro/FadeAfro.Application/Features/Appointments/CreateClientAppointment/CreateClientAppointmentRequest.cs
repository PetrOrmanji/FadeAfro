namespace FadeAfro.Application.Features.Appointments.CreateClientAppointment;

public record CreateClientAppointmentRequest(
    Guid MasterProfileId,
    List<Guid> ServiceIds,
    DateTime StartTime,
    string? Comment);
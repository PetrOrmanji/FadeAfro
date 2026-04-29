namespace FadeAfro.Application.Features.Appointments.CreateAppointment;

public record CreateAppointmentRequest(
    Guid MasterProfileId,
    List<Guid> ServiceIds,
    DateTime StartTime,
    string? Comment);
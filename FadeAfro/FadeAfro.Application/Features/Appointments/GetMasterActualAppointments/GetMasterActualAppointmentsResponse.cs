using FadeAfro.Application.Features.Appointments.Common;

namespace FadeAfro.Application.Features.Appointments.GetMasterActualAppointments;

public record GetMasterActualAppointmentsResponse(List<AppointmentDto> Appointments);

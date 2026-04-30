using FadeAfro.Application.Features.Appointments.Common;

namespace FadeAfro.Application.Features.Appointments.GetMasterActiveAppointments;

public record GetMasterActiveAppointmentsResponse(List<AppointmentDto> Appointments);

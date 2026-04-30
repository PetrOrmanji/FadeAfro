using FadeAfro.Application.Features.Appointments.Common;

namespace FadeAfro.Application.Features.Appointments.GetClientActiveAppointments;

public record GetClientActiveAppointmentsResponse(List<AppointmentDto> Appointments);

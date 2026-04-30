using FadeAfro.Application.Features.Appointments.Common;

namespace FadeAfro.Application.Features.Appointments.GetClientActualAppointments;

public record GetClientActualAppointmentsResponse(List<AppointmentDto> Appointments);

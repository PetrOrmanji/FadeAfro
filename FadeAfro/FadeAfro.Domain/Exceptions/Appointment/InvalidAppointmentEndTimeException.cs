namespace FadeAfro.Domain.Exceptions.Appointment;

public class InvalidAppointmentEndTimeException()
    : DomainException("Appointment end time must be after start time.");

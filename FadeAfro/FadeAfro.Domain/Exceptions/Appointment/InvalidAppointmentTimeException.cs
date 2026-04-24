namespace FadeAfro.Domain.Exceptions.Appointment;

public class InvalidAppointmentTimeException()
    : DomainException("Appointment start time must be in the future.");

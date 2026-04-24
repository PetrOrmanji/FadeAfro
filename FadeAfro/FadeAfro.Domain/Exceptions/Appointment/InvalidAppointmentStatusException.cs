namespace FadeAfro.Domain.Exceptions.Appointment;

public class InvalidAppointmentStatusException()
    : DomainException("Appointment status transition is not allowed.");

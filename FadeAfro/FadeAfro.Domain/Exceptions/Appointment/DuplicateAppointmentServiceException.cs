namespace FadeAfro.Domain.Exceptions.Appointment;

public class DuplicateAppointmentServiceException()
    : DomainException("The same service cannot be added to an appointment twice.");

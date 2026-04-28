namespace FadeAfro.Domain.Exceptions.Appointment;

public class AppointmentMustHaveAtLeastOneServiceException()
    : DomainException("Appointment must have at least one service.");

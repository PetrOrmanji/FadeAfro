namespace FadeAfro.Domain.Exceptions.Appointment;

public class AppointmentMustHaveAtLeastOneServiceException()
    : DomainException("Запись должна содержать хотя бы одну услугу.");

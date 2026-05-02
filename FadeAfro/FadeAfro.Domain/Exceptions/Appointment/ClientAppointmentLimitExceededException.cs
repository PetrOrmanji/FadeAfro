namespace FadeAfro.Domain.Exceptions.Appointment;

public class ClientAppointmentLimitExceededException()
    : DomainException("Превышен лимит активных записей.");
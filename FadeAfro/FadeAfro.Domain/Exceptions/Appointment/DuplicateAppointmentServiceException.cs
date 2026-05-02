namespace FadeAfro.Domain.Exceptions.Appointment;

public class DuplicateAppointmentServiceException()
    : DomainException("Одна и та же услуга не может быть добавлена в запись дважды.");

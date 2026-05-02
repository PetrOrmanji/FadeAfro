namespace FadeAfro.Domain.Exceptions.Appointment;

public class InvalidAppointmentTimeException()
    : DomainException("Время начала записи должно быть в будущем.");

namespace FadeAfro.Domain.Exceptions.Appointment;

public class InvalidAppointmentEndTimeException()
    : DomainException("Время окончания записи должно быть позже времени начала.");

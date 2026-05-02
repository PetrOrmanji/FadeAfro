namespace FadeAfro.Domain.Exceptions.Appointment;

public class AppointmentTimeConflictException()
    : DomainException("Выбранное время уже занято.");

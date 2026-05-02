namespace FadeAfro.Domain.Exceptions.Appointment;

public class AppointmentOfAnotherClient()
    : DomainException("Эта запись принадлежит другому клиенту.");
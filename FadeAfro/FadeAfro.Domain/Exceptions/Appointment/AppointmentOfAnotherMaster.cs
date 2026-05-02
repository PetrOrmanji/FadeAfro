namespace FadeAfro.Domain.Exceptions.Appointment;

public class AppointmentOfAnotherMaster()
    : DomainException("Эта запись принадлежит другому мастеру.");

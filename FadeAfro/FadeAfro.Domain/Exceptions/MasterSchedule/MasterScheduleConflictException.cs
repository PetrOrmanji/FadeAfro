namespace FadeAfro.Domain.Exceptions.MasterSchedule;

public class MasterScheduleConflictException(string message)
    : DomainException(message); 
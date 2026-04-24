namespace FadeAfro.Domain.Exceptions.MasterSchedule;

public class InvalidScheduleTimeException()
    : DomainException("End time must be greater than start time.");

namespace FadeAfro.Domain.Exceptions.MasterSchedule;

public class InvalidMasterScheduleTimeException()
    : DomainException("End time must be greater than start time.");

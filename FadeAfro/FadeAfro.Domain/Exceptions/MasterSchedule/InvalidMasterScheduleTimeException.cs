namespace FadeAfro.Domain.Exceptions.MasterSchedule;

public class InvalidMasterScheduleTimeException()
    : DomainException("Время окончания должно быть позже времени начала.");

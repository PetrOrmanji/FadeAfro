using FadeAfro.Domain.Exceptions.MasterSchedule;

namespace FadeAfro.Domain.Entities;

public class MasterSchedule : Entity
{
    public Guid MasterProfileId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public MasterProfile MasterProfile { get; private set; } = null!;

    public MasterSchedule(Guid masterProfileId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new InvalidScheduleTimeException();

        MasterProfileId = masterProfileId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }
}

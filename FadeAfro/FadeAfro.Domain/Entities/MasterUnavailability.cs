using FadeAfro.Domain.Exceptions.MasterUnavailability;

namespace FadeAfro.Domain.Entities;

public class MasterUnavailability : Entity
{
    public Guid MasterProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }

    public MasterProfile MasterProfile { get; private set; } = null!;

    public MasterUnavailability(Guid masterProfileId, DateOnly date, TimeOnly? startTime, TimeOnly? endTime)
    {
        if (startTime.HasValue && endTime.HasValue && endTime <= startTime)
            throw new InvalidUnavailabilityTimeException();

        MasterProfileId = masterProfileId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }
}

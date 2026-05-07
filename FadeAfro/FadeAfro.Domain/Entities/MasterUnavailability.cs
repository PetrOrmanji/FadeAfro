namespace FadeAfro.Domain.Entities;

public class MasterUnavailability : Entity
{
    public Guid MasterProfileId { get; private set; }
    public DateOnly Date { get; private set; }

    public MasterProfile MasterProfile { get; private set; } = null!;
    
    private MasterUnavailability() { }

    public MasterUnavailability(Guid masterProfileId, DateOnly date)
    {
        MasterProfileId = masterProfileId;
        Date = date;
    }
}

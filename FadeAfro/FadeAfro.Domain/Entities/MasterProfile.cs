namespace FadeAfro.Domain.Entities;

public class MasterProfile : Entity
{
    public Guid MasterId { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Description { get; private set; }

    public User Master { get; private set; } = null!;
    public ICollection<Service> Services { get; private set; } = [];
    public ICollection<MasterSchedule> Schedules { get; private set; } = [];
    public ICollection<Appointment> Appointments { get; private set; } = [];
    public ICollection<MasterUnavailability> Unavailabilities { get; private set; } = [];

    public MasterProfile(Guid masterId, string? photoUrl, string? description)
    {
        MasterId = masterId;
        PhotoUrl = photoUrl;
        Description = description;
    }

    public void UpdatePhotoUrl(string? photoUrl)
    {
        PhotoUrl = photoUrl;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }
}

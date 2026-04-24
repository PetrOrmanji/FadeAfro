namespace FadeAfro.Domain.Entities;

public class MasterProfile : Entity
{
    public Guid MasterId { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Description { get; private set; }

    public User Master { get; private set; } = null!;

    public MasterProfile(Guid masterId, string? photoUrl, string? description)
    {
        MasterId = masterId;
        PhotoUrl = photoUrl;
        Description = description;
    }
}

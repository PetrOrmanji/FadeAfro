using FadeAfro.Domain.Exceptions.Service;

namespace FadeAfro.Domain.Entities;

public class Service : Entity
{
    public Guid MasterProfileId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int Price { get; private set; }
    public TimeSpan Duration { get; private set; }

    public MasterProfile MasterProfile { get; private set; } = null!;
    public ICollection<AppointmentService> AppointmentServices { get; private set; } = [];

    public Service(Guid masterProfileId, string name, string? description, int price, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidServiceNameException();

        if (price <= 0)
            throw new InvalidServicePriceException();

        if (duration <= TimeSpan.Zero)
            throw new InvalidServiceDurationException();

        MasterProfileId = masterProfileId;
        Name = name;
        Description = description;
        Price = price;
        Duration = duration;
    }

    public void Update(string name, string? description, int price, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidServiceNameException();

        if (price <= 0)
            throw new InvalidServicePriceException();

        if (duration <= TimeSpan.Zero)
            throw new InvalidServiceDurationException();

        Name = name;
        Description = description;
        Price = price;
        Duration = duration;
    }
}

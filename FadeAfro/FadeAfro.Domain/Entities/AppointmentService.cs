namespace FadeAfro.Domain.Entities;

public class AppointmentService : Entity
{
    public Guid AppointmentId { get; private set; }
    public Guid? ServiceId { get; private set; }
    public string ServiceName { get; private set; }
    public int Price { get; private set; }
    public TimeSpan Duration { get; private set; }

    public Appointment Appointment { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private AppointmentService() { ServiceName = null!; }

    public AppointmentService(Guid appointmentId, Guid? serviceId, string serviceName, int price, TimeSpan duration)
    {
        AppointmentId = appointmentId;
        ServiceId = serviceId;
        ServiceName = serviceName;
        Price = price;
        Duration = duration;
    }
}

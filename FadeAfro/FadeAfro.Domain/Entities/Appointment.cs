using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Appointment;

namespace FadeAfro.Domain.Entities;

public class Appointment : Entity
{
    public Guid ClientId { get; private set; }
    public Guid MasterProfileId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string? Comment { get; private set; }

    private readonly List<AppointmentService> _services = new();
    public IReadOnlyList<AppointmentService> Services => _services.AsReadOnly();

    public User Client { get; private set; } = null!;
    public MasterProfile MasterProfile { get; private set; } = null!;

    private Appointment() { }

    public Appointment(
        Guid clientId,
        Guid masterProfileId,
        DateTime startTime,
        DateTime endTime,
        string? comment)
    {
        if (startTime <= DateTime.UtcNow)
            throw new InvalidAppointmentTimeException();

        if (endTime <= startTime)
            throw new InvalidAppointmentEndTimeException();

        ClientId = clientId;
        MasterProfileId = masterProfileId;
        StartTime = startTime;
        EndTime = endTime;
        Comment = comment;
    }

    public void AddService(Guid serviceId, string serviceName, int price, TimeSpan duration)
    {
        if (_services.Any(s => s.ServiceId == serviceId))
            throw new DuplicateAppointmentServiceException();

        var appointmentService = new AppointmentService(Id, serviceId, serviceName, price, duration);
        _services.Add(appointmentService);
    }
}

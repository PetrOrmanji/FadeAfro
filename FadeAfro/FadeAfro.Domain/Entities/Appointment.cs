using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Appointment;

namespace FadeAfro.Domain.Entities;

public class Appointment : Entity
{
    public Guid ClientId { get; private set; }
    public Guid MasterProfileId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Comment { get; private set; }

    public User Client { get; private set; } = null!;
    public MasterProfile MasterProfile { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    public Appointment(
        Guid clientId,
        Guid masterProfileId,
        Guid serviceId,
        DateTime startTime,
        DateTime endTime,
        string? comment)
    {
        if (startTime <= DateTime.UtcNow)
            throw new InvalidAppointmentTimeException();

        ClientId = clientId;
        MasterProfileId = masterProfileId;
        ServiceId = serviceId;
        StartTime = startTime;
        EndTime = endTime;
        Status = AppointmentStatus.Pending;
        Comment = comment;
    }
}

using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Appointment;
using FluentAssertions;

namespace FadeAfro.Tests.Domain;

public class AppointmentTests
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid MasterProfileId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly DateTime FutureTime = DateTime.UtcNow.AddDays(1);

    private static Appointment CreateAppointment() =>
        new(ClientId, MasterProfileId, ServiceId, FutureTime, FutureTime.AddMinutes(30), null);

    [Fact]
    public void Constructor_ValidData_CreatesAppointmentWithPendingStatus()
    {
        var appointment = CreateAppointment();

        appointment.ClientId.Should().Be(ClientId);
        appointment.MasterProfileId.Should().Be(MasterProfileId);
        appointment.ServiceId.Should().Be(ServiceId);
        appointment.StartTime.Should().Be(FutureTime);
        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_StartTimeInPast_ThrowsInvalidAppointmentTimeException()
    {
        var pastTime = DateTime.UtcNow.AddHours(-1);

        var act = () => new Appointment(ClientId, MasterProfileId, ServiceId, pastTime, pastTime.AddMinutes(30), null);

        act.Should().Throw<InvalidAppointmentTimeException>();
    }

    [Fact]
    public void Constructor_NullComment_CreatesAppointment()
    {
        var appointment = new Appointment(ClientId, MasterProfileId, ServiceId, FutureTime, FutureTime.AddMinutes(30), null);

        appointment.Comment.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithComment_CreatesAppointment()
    {
        var appointment = new Appointment(ClientId, MasterProfileId, ServiceId, FutureTime, FutureTime.AddMinutes(30), "Window seat please");

        appointment.Comment.Should().Be("Window seat please");
    }

    [Fact]
    public void CancelByClient_FromPending_SetsCancelledByClientStatus()
    {
        var appointment = CreateAppointment();

        appointment.CancelByClient();

        appointment.Status.Should().Be(AppointmentStatus.CancelledByClient);
    }

    [Fact]
    public void CancelByClient_FromConfirmed_SetsCancelledByClientStatus()
    {
        var appointment = CreateAppointment();
        ForceStatus(appointment, AppointmentStatus.Confirmed);

        appointment.CancelByClient();

        appointment.Status.Should().Be(AppointmentStatus.CancelledByClient);
    }

    [Theory]
    [InlineData(AppointmentStatus.CancelledByClient)]
    [InlineData(AppointmentStatus.CancelledByMaster)]
    [InlineData(AppointmentStatus.Completed)]
    public void CancelByClient_FromInvalidStatus_ThrowsInvalidAppointmentStatusException(AppointmentStatus status)
    {
        var appointment = CreateAppointment();
        ForceStatus(appointment, status);

        var act = () => appointment.CancelByClient();

        act.Should().Throw<InvalidAppointmentStatusException>();
    }

    [Fact]
    public void CancelByMaster_FromPending_SetsCancelledByMasterStatus()
    {
        var appointment = CreateAppointment();

        appointment.CancelByMaster();

        appointment.Status.Should().Be(AppointmentStatus.CancelledByMaster);
    }

    [Fact]
    public void CancelByMaster_FromConfirmed_SetsCancelledByMasterStatus()
    {
        var appointment = CreateAppointment();
        ForceStatus(appointment, AppointmentStatus.Confirmed);

        appointment.CancelByMaster();

        appointment.Status.Should().Be(AppointmentStatus.CancelledByMaster);
    }

    [Theory]
    [InlineData(AppointmentStatus.CancelledByClient)]
    [InlineData(AppointmentStatus.CancelledByMaster)]
    [InlineData(AppointmentStatus.Completed)]
    public void CancelByMaster_FromInvalidStatus_ThrowsInvalidAppointmentStatusException(AppointmentStatus status)
    {
        var appointment = CreateAppointment();
        ForceStatus(appointment, status);

        var act = () => appointment.CancelByMaster();

        act.Should().Throw<InvalidAppointmentStatusException>();
    }

    [Fact]
    public void Complete_FromConfirmed_SetsCompletedStatus()
    {
        var appointment = CreateAppointment();
        ForceStatus(appointment, AppointmentStatus.Confirmed);

        appointment.Complete();

        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.CancelledByClient)]
    [InlineData(AppointmentStatus.CancelledByMaster)]
    [InlineData(AppointmentStatus.Completed)]
    public void Complete_FromInvalidStatus_ThrowsInvalidAppointmentStatusException(AppointmentStatus status)
    {
        var appointment = CreateAppointment();
        ForceStatus(appointment, status);

        var act = () => appointment.Complete();

        act.Should().Throw<InvalidAppointmentStatusException>();
    }

    private static void ForceStatus(Appointment appointment, AppointmentStatus status)
    {
        typeof(Appointment)
            .GetProperty(nameof(Appointment.Status))!
            .SetValue(appointment, status);
    }
}

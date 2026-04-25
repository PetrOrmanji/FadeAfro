using FadeAfro.Application.Features.MasterProfiles.GetAvailableSlots;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterProfiles;

public class GetAvailableSlotsHandlerTests
{
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly IMasterScheduleRepository _masterScheduleRepository = Substitute.For<IMasterScheduleRepository>();
    private readonly IMasterUnavailabilityRepository _masterUnavailabilityRepository = Substitute.For<IMasterUnavailabilityRepository>();
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly GetAvailableSlotsHandler _handler;

    private static readonly Guid MasterProfileId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly DateOnly TestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
    private static readonly DayOfWeek TestDay = TestDate.DayOfWeek;

    public GetAvailableSlotsHandlerTests()
    {
        _handler = new GetAvailableSlotsHandler(
            _masterProfileRepository,
            _serviceRepository,
            _masterScheduleRepository,
            _masterUnavailabilityRepository,
            _appointmentRepository);
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
    }

    [Fact]
    public async Task Handle_ServiceNotFound_ThrowsServiceNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(MasterProfileId).Returns(new MasterProfile(MasterProfileId, null, null));
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Service?)null);

        var act = async () => await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        await act.Should().ThrowAsync<ServiceNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoScheduleForDay_ReturnsEmptySlots()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        _masterScheduleRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns([]);

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FullDayUnavailability_ReturnsEmptySlots()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        _masterUnavailabilityRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns(
        [
            new MasterUnavailability(MasterProfileId, TestDate, null, null)
        ]);

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoConflicts_ReturnsAllSlots()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        SetupNoUnavailabilities();
        SetupNoAppointments();

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().HaveCount(4);
        result.Slots[0].Should().Be(new TimeSlotResponse(new TimeOnly(9, 0), new TimeOnly(9, 30)));
        result.Slots[1].Should().Be(new TimeSlotResponse(new TimeOnly(9, 30), new TimeOnly(10, 0)));
        result.Slots[2].Should().Be(new TimeSlotResponse(new TimeOnly(10, 0), new TimeOnly(10, 30)));
        result.Slots[3].Should().Be(new TimeSlotResponse(new TimeOnly(10, 30), new TimeOnly(11, 0)));
    }

    [Fact]
    public async Task Handle_TimeRangeUnavailability_FiltersConflictingSlots()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        _masterUnavailabilityRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns(
        [
            new MasterUnavailability(MasterProfileId, TestDate, new TimeOnly(9, 15), new TimeOnly(10, 0))
        ]);
        SetupNoAppointments();

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().HaveCount(2);
        result.Slots[0].Start.Should().Be(new TimeOnly(10, 0));
        result.Slots[1].Start.Should().Be(new TimeOnly(10, 30));
    }

    [Fact]
    public async Task Handle_ActiveAppointment_FiltersConflictingSlot()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        SetupNoUnavailabilities();

        var appointmentStart = new DateTime(TestDate.Year, TestDate.Month, TestDate.Day, 9, 0, 0, DateTimeKind.Utc);
        var appointment = new Appointment(Guid.NewGuid(), MasterProfileId, Guid.NewGuid(), appointmentStart, appointmentStart.AddMinutes(30), null);
        _appointmentRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns([appointment]);

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().HaveCount(3);
        result.Slots.Should().NotContain(s => s.Start == new TimeOnly(9, 0));
    }

    [Fact]
    public async Task Handle_CancelledAppointment_DoesNotFilterSlot()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        SetupNoUnavailabilities();

        var appointmentStart = new DateTime(TestDate.Year, TestDate.Month, TestDate.Day, 9, 0, 0, DateTimeKind.Utc);
        var appointment = new Appointment(Guid.NewGuid(), MasterProfileId, Guid.NewGuid(), appointmentStart, appointmentStart.AddMinutes(30), null);
        ForceStatus(appointment, AppointmentStatus.CancelledByClient);
        _appointmentRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns([appointment]);

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_AppointmentOnDifferentDate_DoesNotFilterSlot()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        SetupNoUnavailabilities();

        var otherDate = TestDate.AddDays(1);
        var appointmentStart = new DateTime(otherDate.Year, otherDate.Month, otherDate.Day, 9, 0, 0, DateTimeKind.Utc);
        var appointment = new Appointment(Guid.NewGuid(), MasterProfileId, Guid.NewGuid(), appointmentStart, appointmentStart.AddMinutes(30), null);
        _appointmentRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns([appointment]);

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_UnavailabilityOnDifferentDate_DoesNotFilterSlots()
    {
        SetupValidProfileAndService(TimeSpan.FromMinutes(30));
        SetupSchedule(new TimeOnly(9, 0), new TimeOnly(11, 0));
        _masterUnavailabilityRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns(
        [
            new MasterUnavailability(MasterProfileId, TestDate.AddDays(1), null, null)
        ]);
        SetupNoAppointments();

        var result = await _handler.Handle(new GetAvailableSlotsQuery(MasterProfileId, ServiceId, TestDate), CancellationToken.None);

        result.Slots.Should().HaveCount(4);
    }

    private void SetupValidProfileAndService(TimeSpan duration)
    {
        _masterProfileRepository.GetByIdAsync(MasterProfileId).Returns(new MasterProfile(MasterProfileId, null, null));
        _serviceRepository.GetByIdAsync(ServiceId).Returns(new Service(MasterProfileId, "Haircut", null, 500, duration));
    }

    private void SetupSchedule(TimeOnly start, TimeOnly end)
    {
        _masterScheduleRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns(
        [
            new MasterSchedule(MasterProfileId, TestDay, start, end)
        ]);
    }

    private void SetupNoUnavailabilities()
    {
        _masterUnavailabilityRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns([]);
    }

    private void SetupNoAppointments()
    {
        _appointmentRepository.GetByMasterProfileIdAsync(MasterProfileId).Returns([]);
    }

    private static void ForceStatus(Appointment appointment, AppointmentStatus status)
    {
        typeof(Appointment)
            .GetProperty(nameof(Appointment.Status))!
            .SetValue(appointment, status);
    }
}

using FadeAfro.Application.Features.Appointments.CancelAppointment;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Appointments;

public class CancelAppointmentHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly CancelAppointmentHandler _handler;

    private static readonly DateTime FutureTime = DateTime.UtcNow.AddDays(1);

    public CancelAppointmentHandlerTests()
    {
        _handler = new CancelAppointmentHandler(_appointmentRepository);
    }

    [Fact]
    public async Task Handle_CancelByClient_SetsCancelledByClientStatus()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FutureTime, FutureTime.AddMinutes(30), null);

        _appointmentRepository.GetByIdAsync(appointmentId).Returns(appointment);

        await _handler.Handle(new CancelAppointmentCommand(appointmentId, false), CancellationToken.None);

        await _appointmentRepository.Received(1).UpdateAsync(Arg.Is<Appointment>(a =>
            a.Status == AppointmentStatus.CancelledByClient));
    }

    [Fact]
    public async Task Handle_CancelByMaster_SetsCancelledByMasterStatus()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FutureTime, FutureTime.AddMinutes(30), null);

        _appointmentRepository.GetByIdAsync(appointmentId).Returns(appointment);

        await _handler.Handle(new CancelAppointmentCommand(appointmentId, true), CancellationToken.None);

        await _appointmentRepository.Received(1).UpdateAsync(Arg.Is<Appointment>(a =>
            a.Status == AppointmentStatus.CancelledByMaster));
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsAppointmentNotFoundException()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Appointment?)null);

        var act = async () => await _handler.Handle(new CancelAppointmentCommand(Guid.NewGuid(), false), CancellationToken.None);

        await act.Should().ThrowAsync<AppointmentNotFoundException>();
        await _appointmentRepository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>());
    }
}

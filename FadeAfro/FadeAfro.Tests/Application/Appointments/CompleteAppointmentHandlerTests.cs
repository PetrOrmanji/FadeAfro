using FadeAfro.Application.Features.Appointments.CompleteAppointment;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Appointments;

public class CompleteAppointmentHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly CompleteAppointmentHandler _handler;

    private static readonly DateTime FutureTime = DateTime.UtcNow.AddDays(1);

    public CompleteAppointmentHandlerTests()
    {
        _handler = new CompleteAppointmentHandler(_appointmentRepository);
    }

    [Fact]
    public async Task Handle_ConfirmedAppointment_SetsCompletedStatus()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FutureTime, FutureTime.AddMinutes(30), null);
        ForceStatus(appointment, AppointmentStatus.Confirmed);

        _appointmentRepository.GetByIdAsync(appointmentId).Returns(appointment);

        await _handler.Handle(new CompleteAppointmentCommand(appointmentId), CancellationToken.None);

        await _appointmentRepository.Received(1).UpdateAsync(Arg.Is<Appointment>(a =>
            a.Status == AppointmentStatus.Completed));
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsAppointmentNotFoundException()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Appointment?)null);

        var act = async () => await _handler.Handle(new CompleteAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<AppointmentNotFoundException>();
        await _appointmentRepository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>());
    }

    private static void ForceStatus(Appointment appointment, AppointmentStatus status)
    {
        typeof(Appointment)
            .GetProperty(nameof(Appointment.Status))!
            .SetValue(appointment, status);
    }
}

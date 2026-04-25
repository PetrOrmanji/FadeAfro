using FadeAfro.Application.Features.Appointments.CreateAppointment;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Appointments;

public class CreateAppointmentHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly CreateAppointmentHandler _handler;

    private static readonly DateTime FutureTime = DateTime.UtcNow.AddDays(1);

    public CreateAppointmentHandlerTests()
    {
        _handler = new CreateAppointmentHandler(
            _appointmentRepository,
            _userRepository,
            _masterProfileRepository,
            _serviceRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsResponse()
    {
        var clientId = Guid.NewGuid();
        var masterProfileId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var client = new User(123, "Ivan", null, null, [Role.Client]);
        var profile = new MasterProfile(masterProfileId, null, null);
        var service = new Service(masterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));
        var command = new CreateAppointmentCommand(clientId, masterProfileId, serviceId, FutureTime, "Window please");

        _userRepository.GetByIdAsync(clientId).Returns(client);
        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _serviceRepository.GetByIdAsync(serviceId).Returns(service);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _appointmentRepository.Received(1).AddAsync(Arg.Is<Appointment>(a =>
            a.ClientId == clientId &&
            a.MasterProfileId == masterProfileId &&
            a.ServiceId == serviceId &&
            a.StartTime == FutureTime &&
            a.EndTime == FutureTime.AddMinutes(30) &&
            a.Comment == "Window please"));
    }

    [Fact]
    public async Task Handle_ClientNotFound_ThrowsUserNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FutureTime, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        await _appointmentRepository.DidNotReceive().AddAsync(Arg.Any<Appointment>());
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        var clientId = Guid.NewGuid();
        _userRepository.GetByIdAsync(clientId).Returns(new User(123, "Ivan", null, null, [Role.Client]));
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);
        var command = new CreateAppointmentCommand(clientId, Guid.NewGuid(), Guid.NewGuid(), FutureTime, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
        await _appointmentRepository.DidNotReceive().AddAsync(Arg.Any<Appointment>());
    }

    [Fact]
    public async Task Handle_ServiceNotFound_ThrowsServiceNotFoundException()
    {
        var clientId = Guid.NewGuid();
        var masterProfileId = Guid.NewGuid();
        _userRepository.GetByIdAsync(clientId).Returns(new User(123, "Ivan", null, null, [Role.Client]));
        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(new MasterProfile(masterProfileId, null, null));
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Service?)null);
        var command = new CreateAppointmentCommand(clientId, masterProfileId, Guid.NewGuid(), FutureTime, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ServiceNotFoundException>();
        await _appointmentRepository.DidNotReceive().AddAsync(Arg.Any<Appointment>());
    }

    [Fact]
    public async Task Handle_ValidCommand_EndTimeCalculatedFromServiceDuration()
    {
        var clientId = Guid.NewGuid();
        var masterProfileId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var duration = TimeSpan.FromMinutes(90);
        _userRepository.GetByIdAsync(clientId).Returns(new User(123, "Ivan", null, null, [Role.Client]));
        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(new MasterProfile(masterProfileId, null, null));
        _serviceRepository.GetByIdAsync(serviceId).Returns(new Service(masterProfileId, "Long cut", null, 1000, duration));
        var command = new CreateAppointmentCommand(clientId, masterProfileId, serviceId, FutureTime, null);

        await _handler.Handle(command, CancellationToken.None);

        await _appointmentRepository.Received(1).AddAsync(Arg.Is<Appointment>(a =>
            a.EndTime == FutureTime.Add(duration)));
    }
}

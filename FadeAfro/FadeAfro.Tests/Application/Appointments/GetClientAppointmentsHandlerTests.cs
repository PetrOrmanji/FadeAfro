using FadeAfro.Application.Common;
using FadeAfro.Application.Features.Appointments.GetClientAppointments;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Appointments;

public class GetClientAppointmentsHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetClientAppointmentsHandler _handler;

    private static readonly DateTime FutureTime = DateTime.UtcNow.AddDays(1);
    private static readonly PaginationParams DefaultPagination = new(1, 20);

    public GetClientAppointmentsHandlerTests()
    {
        _handler = new GetClientAppointmentsHandler(_appointmentRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ExistingClient_ReturnsPagedResponse()
    {
        var clientId = Guid.NewGuid();
        var client = new User(123, "Ivan", null, null, [Role.Client]);
        var appointments = new List<Appointment>
        {
            new(clientId, Guid.NewGuid(), Guid.NewGuid(), FutureTime, FutureTime.AddMinutes(30), null),
            new(clientId, Guid.NewGuid(), Guid.NewGuid(), FutureTime.AddDays(1), FutureTime.AddDays(1).AddMinutes(30), "Note"),
        };

        _userRepository.GetByIdAsync(clientId).Returns(client);
        _appointmentRepository.GetByClientIdPagedAsync(clientId, 1, 20).Returns((appointments, 2));

        var result = await _handler.Handle(new GetClientAppointmentsQuery(clientId, DefaultPagination), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items[1].Comment.Should().Be("Note");
    }

    [Fact]
    public async Task Handle_ClientNotFound_ThrowsUserNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        var act = async () => await _handler.Handle(new GetClientAppointmentsQuery(Guid.NewGuid(), DefaultPagination), CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoAppointments_ReturnsEmptyPagedResponse()
    {
        var clientId = Guid.NewGuid();
        _userRepository.GetByIdAsync(clientId).Returns(new User(123, "Ivan", null, null, [Role.Client]));
        _appointmentRepository.GetByClientIdPagedAsync(clientId, 1, 20).Returns(([], 0));

        var result = await _handler.Handle(new GetClientAppointmentsQuery(clientId, DefaultPagination), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Pagination_PassesCorrectPageParameters()
    {
        var clientId = Guid.NewGuid();
        var pagination = new PaginationParams(2, 5);
        _userRepository.GetByIdAsync(clientId).Returns(new User(123, "Ivan", null, null, [Role.Client]));
        _appointmentRepository.GetByClientIdPagedAsync(clientId, 2, 5).Returns(([], 0));

        await _handler.Handle(new GetClientAppointmentsQuery(clientId, pagination), CancellationToken.None);

        await _appointmentRepository.Received(1).GetByClientIdPagedAsync(clientId, 2, 5);
    }
}

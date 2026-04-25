using FadeAfro.Application.Common;
using FadeAfro.Application.Features.Appointments.GetMasterAppointments;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Appointments;

public class GetMasterAppointmentsHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository = Substitute.For<IAppointmentRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly GetMasterAppointmentsHandler _handler;

    private static readonly DateTime FutureTime = DateTime.UtcNow.AddDays(1);
    private static readonly PaginationParams DefaultPagination = new(1, 20);

    public GetMasterAppointmentsHandlerTests()
    {
        _handler = new GetMasterAppointmentsHandler(_appointmentRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingMasterProfile_ReturnsPagedResponse()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var appointments = new List<Appointment>
        {
            new(Guid.NewGuid(), masterProfileId, Guid.NewGuid(), FutureTime, FutureTime.AddMinutes(30), null),
        };

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _appointmentRepository.GetByMasterProfileIdPagedAsync(masterProfileId, 1, 20).Returns((appointments, 1));

        var result = await _handler.Handle(new GetMasterAppointmentsQuery(masterProfileId, DefaultPagination), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].ClientId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(new GetMasterAppointmentsQuery(Guid.NewGuid(), DefaultPagination), CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoAppointments_ReturnsEmptyPagedResponse()
    {
        var masterProfileId = Guid.NewGuid();
        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(new MasterProfile(masterProfileId, null, null));
        _appointmentRepository.GetByMasterProfileIdPagedAsync(masterProfileId, 1, 20).Returns(([], 0));

        var result = await _handler.Handle(new GetMasterAppointmentsQuery(masterProfileId, DefaultPagination), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Pagination_PassesCorrectPageParameters()
    {
        var masterProfileId = Guid.NewGuid();
        var pagination = new PaginationParams(3, 10);
        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(new MasterProfile(masterProfileId, null, null));
        _appointmentRepository.GetByMasterProfileIdPagedAsync(masterProfileId, 3, 10).Returns(([], 0));

        await _handler.Handle(new GetMasterAppointmentsQuery(masterProfileId, pagination), CancellationToken.None);

        await _appointmentRepository.Received(1).GetByMasterProfileIdPagedAsync(masterProfileId, 3, 10);
    }
}

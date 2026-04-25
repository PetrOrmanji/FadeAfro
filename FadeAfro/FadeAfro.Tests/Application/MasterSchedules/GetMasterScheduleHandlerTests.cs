using FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterSchedules;

public class GetMasterScheduleHandlerTests
{
    private readonly IMasterScheduleRepository _masterScheduleRepository = Substitute.For<IMasterScheduleRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly GetMasterScheduleHandler _handler;

    public GetMasterScheduleHandlerTests()
    {
        _handler = new GetMasterScheduleHandler(_masterScheduleRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingProfile_ReturnsMappedSchedules()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var schedules = new List<MasterSchedule>
        {
            new(masterProfileId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0)),
            new(masterProfileId, DayOfWeek.Wednesday, new TimeOnly(10, 0), new TimeOnly(19, 0)),
        };

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _masterScheduleRepository.GetByMasterProfileIdAsync(masterProfileId).Returns(schedules);

        var result = await _handler.Handle(new GetMasterScheduleQuery(masterProfileId), CancellationToken.None);

        result.Schedules.Should().HaveCount(2);
        result.Schedules[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Schedules[0].StartTime.Should().Be(new TimeOnly(9, 0));
        result.Schedules[1].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(new GetMasterScheduleQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoSchedules_ReturnsEmptyList()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _masterScheduleRepository.GetByMasterProfileIdAsync(masterProfileId).Returns([]);

        var result = await _handler.Handle(new GetMasterScheduleQuery(masterProfileId), CancellationToken.None);

        result.Schedules.Should().BeEmpty();
    }
}

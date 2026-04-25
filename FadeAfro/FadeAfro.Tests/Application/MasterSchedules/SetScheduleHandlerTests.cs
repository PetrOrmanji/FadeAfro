using FadeAfro.Application.Features.MasterSchedules.SetSchedule;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterSchedules;

public class SetScheduleHandlerTests
{
    private readonly IMasterScheduleRepository _masterScheduleRepository = Substitute.For<IMasterScheduleRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly SetScheduleHandler _handler;

    public SetScheduleHandlerTests()
    {
        _handler = new SetScheduleHandler(_masterScheduleRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsResponse()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var command = new SetScheduleCommand(masterProfileId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _masterScheduleRepository.Received(1).AddAsync(Arg.Is<MasterSchedule>(s =>
            s.MasterProfileId == masterProfileId &&
            s.DayOfWeek == DayOfWeek.Monday &&
            s.StartTime == new TimeOnly(9, 0) &&
            s.EndTime == new TimeOnly(18, 0)));
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        var command = new SetScheduleCommand(Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
        await _masterScheduleRepository.DidNotReceive().AddAsync(Arg.Any<MasterSchedule>());
    }
}

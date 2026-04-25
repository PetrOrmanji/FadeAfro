using FadeAfro.Application.Features.MasterSchedules.DeleteSchedule;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterSchedules;

public class DeleteScheduleHandlerTests
{
    private readonly IMasterScheduleRepository _masterScheduleRepository = Substitute.For<IMasterScheduleRepository>();
    private readonly DeleteScheduleHandler _handler;

    public DeleteScheduleHandlerTests()
    {
        _handler = new DeleteScheduleHandler(_masterScheduleRepository);
    }

    [Fact]
    public async Task Handle_ExistingSchedule_DeletesSchedule()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = new MasterSchedule(Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));

        _masterScheduleRepository.GetByIdAsync(scheduleId).Returns(schedule);

        await _handler.Handle(new DeleteScheduleCommand(scheduleId), CancellationToken.None);

        await _masterScheduleRepository.Received(1).DeleteAsync(scheduleId);
    }

    [Fact]
    public async Task Handle_ScheduleNotFound_ThrowsMasterScheduleNotFoundException()
    {
        _masterScheduleRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterSchedule?)null);

        var act = async () => await _handler.Handle(new DeleteScheduleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<MasterScheduleNotFoundException>();
        await _masterScheduleRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }
}

using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FluentAssertions;

namespace FadeAfro.Tests.Domain;

public class MasterScheduleTests
{
    private static readonly Guid MasterProfileId = Guid.NewGuid();

    [Fact]
    public void Constructor_ValidData_CreatesSchedule()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(18, 0);

        var schedule = new MasterSchedule(MasterProfileId, DayOfWeek.Monday, start, end);

        schedule.MasterProfileId.Should().Be(MasterProfileId);
        schedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
        schedule.StartTime.Should().Be(start);
        schedule.EndTime.Should().Be(end);
        schedule.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_EndTimeEqualToStartTime_ThrowsInvalidScheduleTimeException()
    {
        var time = new TimeOnly(9, 0);

        var act = () => new MasterSchedule(MasterProfileId, DayOfWeek.Monday, time, time);

        act.Should().Throw<InvalidScheduleTimeException>();
    }

    [Fact]
    public void Constructor_EndTimeBeforeStartTime_ThrowsInvalidScheduleTimeException()
    {
        var start = new TimeOnly(18, 0);
        var end = new TimeOnly(9, 0);

        var act = () => new MasterSchedule(MasterProfileId, DayOfWeek.Monday, start, end);

        act.Should().Throw<InvalidScheduleTimeException>();
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Saturday)]
    public void Constructor_AnyDayOfWeek_CreatesSchedule(DayOfWeek day)
    {
        var schedule = new MasterSchedule(MasterProfileId, day, new TimeOnly(9, 0), new TimeOnly(18, 0));

        schedule.DayOfWeek.Should().Be(day);
    }
}

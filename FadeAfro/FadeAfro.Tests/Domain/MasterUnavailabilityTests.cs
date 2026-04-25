using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FluentAssertions;

namespace FadeAfro.Tests.Domain;

public class MasterUnavailabilityTests
{
    private static readonly Guid MasterProfileId = Guid.NewGuid();
    private static readonly DateOnly Date = new(2026, 6, 1);

    [Fact]
    public void Constructor_FullDay_CreatesUnavailability()
    {
        var unavailability = new MasterUnavailability(MasterProfileId, Date, null, null);

        unavailability.MasterProfileId.Should().Be(MasterProfileId);
        unavailability.Date.Should().Be(Date);
        unavailability.StartTime.Should().BeNull();
        unavailability.EndTime.Should().BeNull();
        unavailability.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_TimeRange_CreatesUnavailability()
    {
        var start = new TimeOnly(10, 0);
        var end = new TimeOnly(12, 0);

        var unavailability = new MasterUnavailability(MasterProfileId, Date, start, end);

        unavailability.StartTime.Should().Be(start);
        unavailability.EndTime.Should().Be(end);
    }

    [Fact]
    public void Constructor_EndTimeEqualToStartTime_ThrowsInvalidUnavailabilityTimeException()
    {
        var time = new TimeOnly(10, 0);

        var act = () => new MasterUnavailability(MasterProfileId, Date, time, time);

        act.Should().Throw<InvalidUnavailabilityTimeException>();
    }

    [Fact]
    public void Constructor_EndTimeBeforeStartTime_ThrowsInvalidUnavailabilityTimeException()
    {
        var start = new TimeOnly(14, 0);
        var end = new TimeOnly(10, 0);

        var act = () => new MasterUnavailability(MasterProfileId, Date, start, end);

        act.Should().Throw<InvalidUnavailabilityTimeException>();
    }
}

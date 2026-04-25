using FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterUnavailabilities;

public class AddUnavailabilityHandlerTests
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository = Substitute.For<IMasterUnavailabilityRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly AddUnavailabilityHandler _handler;

    private static readonly DateOnly Date = new(2026, 6, 1);

    public AddUnavailabilityHandlerTests()
    {
        _handler = new AddUnavailabilityHandler(_unavailabilityRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_FullDayUnavailability_CreatesAndReturnsResponse()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var command = new AddUnavailabilityCommand(masterProfileId, Date, null, null);

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _unavailabilityRepository.Received(1).AddAsync(Arg.Is<MasterUnavailability>(u =>
            u.MasterProfileId == masterProfileId &&
            u.Date == Date &&
            u.StartTime == null &&
            u.EndTime == null));
    }

    [Fact]
    public async Task Handle_TimeRangeUnavailability_CreatesAndReturnsResponse()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var start = new TimeOnly(10, 0);
        var end = new TimeOnly(14, 0);
        var command = new AddUnavailabilityCommand(masterProfileId, Date, start, end);

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _unavailabilityRepository.Received(1).AddAsync(Arg.Is<MasterUnavailability>(u =>
            u.StartTime == start && u.EndTime == end));
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        var command = new AddUnavailabilityCommand(Guid.NewGuid(), Date, null, null);
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
        await _unavailabilityRepository.DidNotReceive().AddAsync(Arg.Any<MasterUnavailability>());
    }
}

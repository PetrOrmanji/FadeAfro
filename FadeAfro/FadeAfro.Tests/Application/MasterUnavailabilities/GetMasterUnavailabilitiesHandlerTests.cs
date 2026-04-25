using FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterUnavailabilities;

public class GetMasterUnavailabilitiesHandlerTests
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository = Substitute.For<IMasterUnavailabilityRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly GetMasterUnavailabilitiesHandler _handler;

    private static readonly DateOnly Date = new(2026, 6, 1);

    public GetMasterUnavailabilitiesHandlerTests()
    {
        _handler = new GetMasterUnavailabilitiesHandler(_unavailabilityRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingProfile_ReturnsMappedUnavailabilities()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var unavailabilities = new List<MasterUnavailability>
        {
            new(masterProfileId, Date, null, null),
            new(masterProfileId, Date.AddDays(1), new TimeOnly(10, 0), new TimeOnly(12, 0)),
        };

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _unavailabilityRepository.GetByMasterProfileIdAsync(masterProfileId).Returns(unavailabilities);

        var result = await _handler.Handle(new GetMasterUnavailabilitiesQuery(masterProfileId), CancellationToken.None);

        result.Unavailabilities.Should().HaveCount(2);
        result.Unavailabilities[0].Date.Should().Be(Date);
        result.Unavailabilities[0].StartTime.Should().BeNull();
        result.Unavailabilities[1].StartTime.Should().Be(new TimeOnly(10, 0));
        result.Unavailabilities[1].EndTime.Should().Be(new TimeOnly(12, 0));
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(new GetMasterUnavailabilitiesQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoUnavailabilities_ReturnsEmptyList()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _unavailabilityRepository.GetByMasterProfileIdAsync(masterProfileId).Returns([]);

        var result = await _handler.Handle(new GetMasterUnavailabilitiesQuery(masterProfileId), CancellationToken.None);

        result.Unavailabilities.Should().BeEmpty();
    }
}

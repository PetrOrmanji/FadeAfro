using FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterProfiles;

public class GetMasterProfileHandlerTests
{
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly GetMasterProfileHandler _handler;

    public GetMasterProfileHandlerTests()
    {
        _handler = new GetMasterProfileHandler(_masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingProfile_ReturnsResponse()
    {
        var masterId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var profile = new MasterProfile(masterId, "https://photo.url", "Bio");
        SetId(profile, profileId);

        _masterProfileRepository.GetByIdAsync(profileId).Returns(profile);

        var result = await _handler.Handle(new GetMasterProfileQuery(profileId), CancellationToken.None);

        result.Id.Should().Be(profileId);
        result.MasterId.Should().Be(masterId);
        result.PhotoUrl.Should().Be("https://photo.url");
        result.Description.Should().Be("Bio");
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(new GetMasterProfileQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
    }

    private static void SetId(MasterProfile profile, Guid id)
    {
        typeof(Entity)
            .GetProperty(nameof(Entity.Id))!
            .SetValue(profile, id);
    }
}

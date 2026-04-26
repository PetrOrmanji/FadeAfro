using FadeAfro.Application.Features.MasterProfiles.GetAllMasters;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterProfiles;

public class GetAllMastersHandlerTests
{
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly GetAllMastersHandler _handler;

    public GetAllMastersHandlerTests()
    {
        _handler = new GetAllMastersHandler(_masterProfileRepository);
    }

    [Fact]
    public async Task Handle_MultipleProfiles_ReturnsMappedResponse()
    {
        var user1 = new User(111, "Ivan", null, null, [Role.Master]);
        var user2 = new User(222, "Anna", null, null, [Role.Master]);
        var profile1 = new MasterProfile(Guid.NewGuid(), "https://photo1.url", "Bio 1");
        var profile2 = new MasterProfile(Guid.NewGuid(), "https://photo2.url", "Bio 2");
        SetMaster(profile1, user1);
        SetMaster(profile2, user2);

        _masterProfileRepository.GetAllAsync().Returns([profile1, profile2]);

        var result = await _handler.Handle(new GetAllMastersQuery(), CancellationToken.None);

        result.Masters.Should().HaveCount(2);
        result.Masters[0].PhotoUrl.Should().Be("https://photo1.url");
        result.Masters[1].PhotoUrl.Should().Be("https://photo2.url");
    }

    [Fact]
    public async Task Handle_EmptyRepository_ReturnsEmptyList()
    {
        _masterProfileRepository.GetAllAsync().Returns([]);

        var result = await _handler.Handle(new GetAllMastersQuery(), CancellationToken.None);

        result.Masters.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Profile_MapsMasterIdCorrectly()
    {
        var masterId = Guid.NewGuid();
        var user = new User(123, "Ivan", null, null, [Role.Master]);
        var profile = new MasterProfile(masterId, null, null);
        SetMaster(profile, user);
        _masterProfileRepository.GetAllAsync().Returns([profile]);

        var result = await _handler.Handle(new GetAllMastersQuery(), CancellationToken.None);

        result.Masters.Single().MasterId.Should().Be(masterId);
    }

    private static void SetMaster(MasterProfile profile, User user)
    {
        typeof(MasterProfile)
            .GetProperty(nameof(MasterProfile.Master))!
            .SetValue(profile, user);
    }
}

using FadeAfro.Application.Features.MasterProfiles.GetAllMasters;
using FadeAfro.Domain.Entities;
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
        var profiles = new List<MasterProfile>
        {
            new(Guid.NewGuid(), "https://photo1.url", "Bio 1"),
            new(Guid.NewGuid(), "https://photo2.url", "Bio 2"),
        };
        _masterProfileRepository.GetAllAsync().Returns(profiles);

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
        var profile = new MasterProfile(masterId, null, null);
        _masterProfileRepository.GetAllAsync().Returns([profile]);

        var result = await _handler.Handle(new GetAllMastersQuery(), CancellationToken.None);

        result.Masters.Single().MasterId.Should().Be(masterId);
    }
}

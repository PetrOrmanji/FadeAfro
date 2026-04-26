using FadeAfro.Application.Features.MasterProfiles.UpdateMasterDescription;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterProfiles;

public class UpdateMasterDescriptionHandlerTests
{
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly UpdateMasterDescriptionHandler _handler;

    public UpdateMasterDescriptionHandlerTests()
    {
        _handler = new UpdateMasterDescriptionHandler(_masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingProfile_UpdatesDescription()
    {
        var profileId = Guid.NewGuid();
        var profile = new MasterProfile(Guid.NewGuid(), "photo.url", "Old bio");
        var command = new UpdateMasterDescriptionCommand(profileId, "New bio");

        _masterProfileRepository.GetByIdAsync(profileId).Returns(profile);

        await _handler.Handle(command, CancellationToken.None);

        await _masterProfileRepository.Received(1).UpdateAsync(Arg.Is<MasterProfile>(mp =>
            mp.Description == "New bio"));
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        var command = new UpdateMasterDescriptionCommand(Guid.NewGuid(), null);
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
        await _masterProfileRepository.DidNotReceive().UpdateAsync(Arg.Any<MasterProfile>());
    }

    [Fact]
    public async Task Handle_NullDescription_ClearsDescription()
    {
        var profileId = Guid.NewGuid();
        var profile = new MasterProfile(Guid.NewGuid(), "photo.url", "Old bio");
        var command = new UpdateMasterDescriptionCommand(profileId, null);

        _masterProfileRepository.GetByIdAsync(profileId).Returns(profile);

        await _handler.Handle(command, CancellationToken.None);

        await _masterProfileRepository.Received(1).UpdateAsync(Arg.Is<MasterProfile>(mp =>
            mp.Description == null));
    }
}

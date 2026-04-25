using FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfile;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterProfiles;

public class UpdateMasterProfileHandlerTests
{
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly UpdateMasterProfileHandler _handler;

    public UpdateMasterProfileHandlerTests()
    {
        _handler = new UpdateMasterProfileHandler(_masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingProfile_UpdatesProfile()
    {
        var profileId = Guid.NewGuid();
        var profile = new MasterProfile(Guid.NewGuid(), "old-photo.url", "Old bio");
        var command = new UpdateMasterProfileCommand(profileId, "new-photo.url", "New bio");

        _masterProfileRepository.GetByIdAsync(profileId).Returns(profile);

        await _handler.Handle(command, CancellationToken.None);

        await _masterProfileRepository.Received(1).UpdateAsync(Arg.Is<MasterProfile>(mp =>
            mp.PhotoUrl == "new-photo.url" &&
            mp.Description == "New bio"));
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        var command = new UpdateMasterProfileCommand(Guid.NewGuid(), null, null);
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
        await _masterProfileRepository.DidNotReceive().UpdateAsync(Arg.Any<MasterProfile>());
    }

    [Fact]
    public async Task Handle_NullPhotoAndDescription_UpdatesProfileWithNulls()
    {
        var profileId = Guid.NewGuid();
        var profile = new MasterProfile(Guid.NewGuid(), "old.url", "Old bio");
        var command = new UpdateMasterProfileCommand(profileId, null, null);

        _masterProfileRepository.GetByIdAsync(profileId).Returns(profile);

        await _handler.Handle(command, CancellationToken.None);

        await _masterProfileRepository.Received(1).UpdateAsync(Arg.Is<MasterProfile>(mp =>
            mp.PhotoUrl == null && mp.Description == null));
    }
}

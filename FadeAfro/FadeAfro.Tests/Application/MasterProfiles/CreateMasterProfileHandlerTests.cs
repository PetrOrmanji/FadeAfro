using FadeAfro.Application.Features.MasterProfiles.CreateMasterProfile;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterProfiles;

public class CreateMasterProfileHandlerTests
{
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly CreateMasterProfileHandler _handler;

    public CreateMasterProfileHandlerTests()
    {
        _handler = new CreateMasterProfileHandler(_masterProfileRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsResponse()
    {
        var masterId = Guid.NewGuid();
        var user = new User(123, "Ivan", "Petrov", "ivanp", [Role.Client]);
        var command = new CreateMasterProfileCommand(masterId, "https://photo.url", "Bio");

        _userRepository.GetByIdAsync(masterId).Returns(user);
        _masterProfileRepository.GetByMasterIdAsync(masterId).Returns((MasterProfile?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _masterProfileRepository.Received(1).AddAsync(Arg.Is<MasterProfile>(mp =>
            mp.MasterId == masterId &&
            mp.PhotoUrl == "https://photo.url" &&
            mp.Description == "Bio"));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUserNotFoundException()
    {
        var command = new CreateMasterProfileCommand(Guid.NewGuid(), null, null);
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        await _masterProfileRepository.DidNotReceive().AddAsync(Arg.Any<MasterProfile>());
    }

    [Fact]
    public async Task Handle_ProfileAlreadyExists_ThrowsMasterProfileAlreadyExistsException()
    {
        var masterId = Guid.NewGuid();
        var user = new User(123, "Ivan", "Petrov", "ivanp", [Role.Master]);
        var existingProfile = new MasterProfile(masterId, null, null);
        var command = new CreateMasterProfileCommand(masterId, null, null);

        _userRepository.GetByIdAsync(masterId).Returns(user);
        _masterProfileRepository.GetByMasterIdAsync(masterId).Returns(existingProfile);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileAlreadyExistsException>();
        await _masterProfileRepository.DidNotReceive().AddAsync(Arg.Any<MasterProfile>());
    }

    [Fact]
    public async Task Handle_NullPhotoAndDescription_CreatesProfile()
    {
        var masterId = Guid.NewGuid();
        var user = new User(123, "Ivan", null, null, [Role.Client]);
        var command = new CreateMasterProfileCommand(masterId, null, null);

        _userRepository.GetByIdAsync(masterId).Returns(user);
        _masterProfileRepository.GetByMasterIdAsync(masterId).Returns((MasterProfile?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _masterProfileRepository.Received(1).AddAsync(Arg.Is<MasterProfile>(mp =>
            mp.PhotoUrl == null && mp.Description == null));
    }
}

using FadeAfro.Application.Features.Users.RegisterUser;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Users;

public class RegisterUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_NewUser_CreatesAndReturnsResponse()
    {
        var command = new RegisterUserCommand(123456789, "Ivan", "Petrov", "ivanp");
        _userRepository.GetByTelegramIdAsync(command.TelegramId).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
            u.TelegramId == command.TelegramId &&
            u.FirstName == command.FirstName &&
            u.Roles.Contains(Role.Client)));
    }

    [Fact]
    public async Task Handle_ExistingUser_ThrowsUserAlreadyExistsException()
    {
        var existingUser = new User(123456789, "Ivan", null, null, [Role.Client]);
        var command = new RegisterUserCommand(123456789, "Ivan", null, null);
        _userRepository.GetByTelegramIdAsync(command.TelegramId).Returns(existingUser);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }
}

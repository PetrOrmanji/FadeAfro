using FadeAfro.Application.Features.Users.GetUser;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Users;

public class GetUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _handler = new GetUserHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserResponse()
    {
        var user = new User(123456789, "Ivan", "Petrov", "ivanp", [Role.Client]);
        _userRepository.GetByTelegramIdAsync(123456789).Returns(user);

        var result = await _handler.Handle(new GetUserQuery(123456789), CancellationToken.None);

        result.TelegramId.Should().Be(123456789);
        result.FirstName.Should().Be("Ivan");
        result.LastName.Should().Be("Petrov");
        result.Username.Should().Be("ivanp");
        result.Roles.Should().ContainSingle(r => r == Role.Client);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsUserNotFoundException()
    {
        _userRepository.GetByTelegramIdAsync(Arg.Any<long>()).Returns((User?)null);

        var act = async () => await _handler.Handle(new GetUserQuery(999), CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }
}

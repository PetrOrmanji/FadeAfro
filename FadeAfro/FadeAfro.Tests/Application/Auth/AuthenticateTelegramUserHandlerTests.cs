using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FadeAfro.Application.Features.Auth.AuthenticateTelegramUser;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Auth;
using FadeAfro.Domain.Repositories;
using FadeAfro.Domain.Services;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Auth;

public class AuthenticateTelegramUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly ITelegramSettings _telegramSettings = Substitute.For<ITelegramSettings>();
    private readonly AuthenticateTelegramUserHandler _handler;

    private const string BotToken = "test-bot-token";

    public AuthenticateTelegramUserHandlerTests()
    {
        _telegramSettings.BotToken.Returns(BotToken);
        _jwtTokenService.GenerateToken(Arg.Any<User>()).Returns("jwt-token");
        _handler = new AuthenticateTelegramUserHandler(_userRepository, _jwtTokenService, _telegramSettings);
    }

    [Fact]
    public async Task Handle_NewUser_CreatesUserAndReturnsToken()
    {
        var initData = BuildValidInitData(BotToken, 123456789, "Ivan", "Petrov", "ivanp");
        _userRepository.GetByTelegramIdAsync(123456789).Returns((User?)null);

        var result = await _handler.Handle(new AuthenticateTelegramUserCommand(initData), CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
            u.TelegramId == 123456789 &&
            u.FirstName == "Ivan" &&
            u.Roles.Contains(Role.Client)));
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsTokenWithoutCreating()
    {
        var initData = BuildValidInitData(BotToken, 123456789, "Ivan", null, null);
        var existingUser = new User(123456789, "Ivan", null, null, [Role.Client]);
        _userRepository.GetByTelegramIdAsync(123456789).Returns(existingUser);

        var result = await _handler.Handle(new AuthenticateTelegramUserCommand(initData), CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_InvalidHash_ThrowsInvalidInitDataException()
    {
        var initData = BuildValidInitData(BotToken, 123456789, "Ivan", null, null)
            .Replace("hash=", "hash=tampered");

        var act = async () => await _handler.Handle(new AuthenticateTelegramUserCommand(initData), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidInitDataException>();
    }

    [Fact]
    public async Task Handle_MissingHash_ThrowsInvalidInitDataException()
    {
        var initData = "user=%7B%22id%22%3A123%7D&auth_date=1000000000";

        var act = async () => await _handler.Handle(new AuthenticateTelegramUserCommand(initData), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidInitDataException>();
    }

    [Fact]
    public async Task Handle_MissingUserField_ThrowsInvalidInitDataException()
    {
        var initData = BuildValidInitDataWithoutUser(BotToken);

        var act = async () => await _handler.Handle(new AuthenticateTelegramUserCommand(initData), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidInitDataException>();
    }

    [Fact]
    public async Task Handle_ValidInitData_CallsGenerateTokenWithCorrectUser()
    {
        var initData = BuildValidInitData(BotToken, 999, "Anna", "Smith", "annas");
        _userRepository.GetByTelegramIdAsync(999).Returns((User?)null);

        await _handler.Handle(new AuthenticateTelegramUserCommand(initData), CancellationToken.None);

        _jwtTokenService.Received(1).GenerateToken(Arg.Is<User>(u =>
            u.TelegramId == 999 &&
            u.FirstName == "Anna" &&
            u.LastName == "Smith" &&
            u.Username == "annas"));
    }

    private static string BuildValidInitData(string botToken, long userId, string firstName, string? lastName, string? username)
    {
        var userDict = new Dictionary<string, object> { ["id"] = userId, ["first_name"] = firstName };
        if (lastName != null) userDict["last_name"] = lastName;
        if (username != null) userDict["username"] = username;

        var userJson = JsonSerializer.Serialize(userDict);
        var pairs = new Dictionary<string, string> { ["user"] = userJson, ["auth_date"] = "1000000000" };

        var hash = ComputeHash(botToken, pairs);
        pairs["hash"] = hash;

        return string.Join("&", pairs.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    private static string BuildValidInitDataWithoutUser(string botToken)
    {
        var pairs = new Dictionary<string, string> { ["auth_date"] = "1000000000" };
        var hash = ComputeHash(botToken, pairs);
        pairs["hash"] = hash;

        return string.Join("&", pairs.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    private static string ComputeHash(string botToken, Dictionary<string, string> pairs)
    {
        var dataCheckString = pairs
            .OrderBy(p => p.Key)
            .Select(p => $"{p.Key}={p.Value}")
            .Aggregate((a, b) => $"{a}\n{b}");

        var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(botToken));
        return Convert.ToHexString(HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString))).ToLower();
    }
}

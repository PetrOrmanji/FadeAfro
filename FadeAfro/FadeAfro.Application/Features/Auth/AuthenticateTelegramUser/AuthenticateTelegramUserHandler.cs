using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.Auth;
using FadeAfro.Domain.Repositories;
using FadeAfro.Domain.Services;
using MediatR;

namespace FadeAfro.Application.Features.Auth.AuthenticateTelegramUser;

public class AuthenticateTelegramUserHandler : IRequestHandler<AuthenticateTelegramUserCommand, AuthenticateTelegramUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITelegramSettings _telegramSettings;

    public AuthenticateTelegramUserHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ITelegramSettings telegramSettings)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _telegramSettings = telegramSettings;
    }

    public async Task<AuthenticateTelegramUserResponse> Handle(
        AuthenticateTelegramUserCommand command,
        CancellationToken cancellationToken)
    {
        ValidateInitData(command.InitData);

        var userData = ParseUserData(command.InitData);

        var user = await _userRepository.GetByTelegramIdAsync(userData.TelegramId);

        if (user is null)
        {
            user = new User(
                userData.TelegramId,
                userData.FirstName,
                userData.LastName,
                userData.Username,
                [Role.Client]);

            await _userRepository.AddAsync(user);
        }
        else
        {
            if (user.Roles.Contains(Role.Master))
            {
                user.UpdateUserName(userData.Username);
            }
            else
            {
                user.Update(userData.FirstName, userData.LastName, userData.Username);
            }
            
            await _userRepository.UpdateAsync(user);
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthenticateTelegramUserResponse(token);
    }

    private void ValidateInitData(string initData)
    {
        if (_telegramSettings.SkipValidation)
            return;

        var pairs = initData
            .Split('&')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => Uri.UnescapeDataString(p[1]));

        if (!pairs.TryGetValue("hash", out var hash))
            throw new InvalidInitDataException("hash is missing.");

        var dataCheckString = pairs
            .Where(p => p.Key != "hash")
            .OrderBy(p => p.Key)
            .Select(p => $"{p.Key}={p.Value}")
            .Aggregate((a, b) => $"{a}\n{b}");

        var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(_telegramSettings.BotToken));
        var expectedHash = Convert.ToHexString(HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString))).ToLower();

        if (expectedHash != hash)
            throw new InvalidInitDataException("hash mismatch.");
    }

    private static TelegramUserData ParseUserData(string initData)
    {
        var pairs = initData
            .Split('&')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => Uri.UnescapeDataString(p[1]));

        if (!pairs.TryGetValue("user", out var userJson))
            throw new InvalidInitDataException("user is missing.");

        var doc = JsonDocument.Parse(userJson);
        var root = doc.RootElement;

        return new TelegramUserData(
            root.GetProperty("id").GetInt64(),
            root.GetProperty("first_name").GetString()!,
            root.TryGetProperty("last_name", out var ln) ? ln.GetString() : null,
            root.TryGetProperty("username", out var un) ? un.GetString() : null);
    }

    private record TelegramUserData(long TelegramId, string FirstName, string? LastName, string? Username);
}

using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Users.Common;

public record UserDto(
    Guid Id,
    long TelegramId,
    string FirstName,
    string? LastName,
    string? Username,
    IReadOnlyList<Role> Roles);
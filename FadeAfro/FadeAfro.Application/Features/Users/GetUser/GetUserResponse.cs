using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Users.GetUser;

public record GetUserResponse(
    Guid Id,
    long TelegramId,
    string FirstName,
    string? LastName,
    string? Username,
    List<Role> Roles);

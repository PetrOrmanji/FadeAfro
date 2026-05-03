using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Users.GetUsers;

public record GetUsersResponse(
    Guid Id,
    long TelegramId,
    string FirstName,
    string? LastName,
    string? Username,
    IReadOnlyList<Role> Roles);
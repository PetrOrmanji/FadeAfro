using FadeAfro.Domain.Enums;

namespace FadeAfro.Application.Features.Users.GetAllUsers;

public record GetAllUsersResponse(
    Guid Id,
    long TelegramId,
    string FirstName,
    string? LastName,
    string? Username,
    IReadOnlyList<Role> Roles);
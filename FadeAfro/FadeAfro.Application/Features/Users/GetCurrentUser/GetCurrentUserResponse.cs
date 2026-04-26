namespace FadeAfro.Application.Features.Users.GetCurrentUser;

public record GetCurrentUserResponse(
    Guid Id,
    string FirstName,
    string? LastName);

namespace FadeAfro.Application.Features.Users.GetCurrentUser;

public record GetUserResponse(
    Guid Id,
    string FirstName,
    string? LastName);

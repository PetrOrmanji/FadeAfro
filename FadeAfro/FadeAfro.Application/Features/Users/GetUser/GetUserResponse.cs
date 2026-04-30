namespace FadeAfro.Application.Features.Users.GetUser;

public record GetUserResponse(
    Guid Id,
    string FirstName,
    string? LastName);

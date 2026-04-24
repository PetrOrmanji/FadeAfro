using MediatR;

namespace FadeAfro.Application.Features.Users.RegisterUser;

public record RegisterUserCommand(
    long TelegramId,
    string FirstName,
    string? LastName,
    string? Username) : IRequest<RegisterUserResponse>;

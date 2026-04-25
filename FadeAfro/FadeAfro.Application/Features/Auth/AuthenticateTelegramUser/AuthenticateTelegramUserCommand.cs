using MediatR;

namespace FadeAfro.Application.Features.Auth.AuthenticateTelegramUser;

public record AuthenticateTelegramUserCommand(string InitData) : IRequest<AuthenticateTelegramUserResponse>;

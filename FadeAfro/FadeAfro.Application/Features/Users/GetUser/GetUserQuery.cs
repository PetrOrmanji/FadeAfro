using MediatR;

namespace FadeAfro.Application.Features.Users.GetUser;

public record GetUserQuery(long TelegramId) : IRequest<GetUserResponse>;

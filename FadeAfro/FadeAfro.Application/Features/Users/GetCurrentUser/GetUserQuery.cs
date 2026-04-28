using MediatR;

namespace FadeAfro.Application.Features.Users.GetCurrentUser;

public record GetUserQuery(Guid UserId) : IRequest<GetUserResponse>;

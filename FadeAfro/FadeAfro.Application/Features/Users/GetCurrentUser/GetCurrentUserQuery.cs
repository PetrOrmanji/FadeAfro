using MediatR;

namespace FadeAfro.Application.Features.Users.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<GetCurrentUserResponse>;

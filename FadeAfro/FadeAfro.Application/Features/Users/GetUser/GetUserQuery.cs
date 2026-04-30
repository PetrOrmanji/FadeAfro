using MediatR;

namespace FadeAfro.Application.Features.Users.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<GetUserResponse>;

using MediatR;

namespace FadeAfro.Application.Features.Users.GetOwners;

public record GetOwnersQuery : IRequest<GetOwnersResponse>;
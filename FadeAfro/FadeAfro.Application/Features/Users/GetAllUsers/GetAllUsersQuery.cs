using FadeAfro.Application.Common;
using FadeAfro.Application.Features.Users.GetUser;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetAllUsers;

public record GetAllUsersQuery(int Page, int PageSize) : IRequest<PagedResponse<GetUserResponse>>;

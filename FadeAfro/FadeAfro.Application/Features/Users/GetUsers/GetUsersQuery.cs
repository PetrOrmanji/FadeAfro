using FadeAfro.Application.Common;
using FadeAfro.Application.Features.Users.Common;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetUsers;

public record GetUsersQuery(int Page, int PageSize, string? Search) : IRequest<PagedResponse<UserDto>>;

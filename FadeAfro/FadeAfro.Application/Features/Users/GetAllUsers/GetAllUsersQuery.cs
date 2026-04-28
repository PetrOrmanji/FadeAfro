using FadeAfro.Application.Common;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetAllUsers;

public record GetAllUsersQuery(int Page, int PageSize, string? Search) : IRequest<PagedResponse<GetAllUsersResponse>>;

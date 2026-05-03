using FadeAfro.Application.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResponse<GetUsersResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<GetUsersResponse>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var totalCount = await _userRepository.CountAsync(query.Search);
        var users = await _userRepository.GetAllAsync(query.Page, query.PageSize, query.Search);

        var items = users
            .Select(u => new GetUsersResponse(
                u.Id,
                u.TelegramId,
                u.FirstName,
                u.LastName,
                u.Username,
                u.Roles))
            .ToList();

        return new PagedResponse<GetUsersResponse>(items, totalCount, query.Page, query.PageSize);
    }
}

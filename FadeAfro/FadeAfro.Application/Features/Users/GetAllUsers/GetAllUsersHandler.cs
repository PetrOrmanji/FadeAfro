using FadeAfro.Application.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<GetAllUsersResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<GetAllUsersResponse>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var totalCount = await _userRepository.CountAsync(query.Search);
        var users = await _userRepository.GetAllAsync(query.Page, query.PageSize, query.Search);

        var items = users
            .Select(u => new GetAllUsersResponse(
                u.Id,
                u.TelegramId,
                u.FirstName,
                u.LastName,
                u.Username,
                u.Roles))
            .ToList();

        return new PagedResponse<GetAllUsersResponse>(items, totalCount, query.Page, query.PageSize);
    }
}

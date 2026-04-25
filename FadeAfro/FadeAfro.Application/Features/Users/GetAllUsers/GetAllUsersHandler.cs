using FadeAfro.Application.Common;
using FadeAfro.Application.Features.Users.GetUser;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<GetUserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<GetUserResponse>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var totalCount = await _userRepository.CountAsync();
        var users = await _userRepository.GetAllAsync(query.Page, query.PageSize);

        var items = users
            .Select(u => new GetUserResponse(
                u.Id,
                u.TelegramId,
                u.FirstName,
                u.LastName,
                u.Username,
                u.Roles))
            .ToList();

        return new PagedResponse<GetUserResponse>(items, totalCount, query.Page, query.PageSize);
    }
}

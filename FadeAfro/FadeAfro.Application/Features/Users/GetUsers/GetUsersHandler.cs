using FadeAfro.Application.Common;
using FadeAfro.Application.Features.Users.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var totalCount = await _userRepository.CountAsync(query.Search);
        var users = await _userRepository.GetAllAsync(query.Page, query.PageSize, query.Search);
        
        var userDtoList = new List<UserDto>();

        foreach (var user in users)
        {
            var userDto = new UserDto(
                user.Id,
                user.TelegramId,
                user.FirstName,
                user.LastName,
                user.Username,
                user.Roles);
            
            userDtoList.Add(userDto);
        }
        
        return new PagedResponse<UserDto>(userDtoList, totalCount, query.Page, query.PageSize);
    }
}

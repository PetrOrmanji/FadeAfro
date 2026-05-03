using FadeAfro.Application.Features.Users.Common;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetOwners;

public class GetOwnersHandler : IRequestHandler<GetOwnersQuery, GetOwnersResponse>
{
    private readonly IUserRepository _userRepository;
    
    public GetOwnersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<GetOwnersResponse> Handle(GetOwnersQuery request, CancellationToken cancellationToken)
    {
        var owners = await _userRepository.GetByRoleAsync(Role.Owner);

        var ownerDtoList = new List<UserDto>();

        foreach (var owner in owners)
        {
            var ownerDto = new UserDto(
                owner.Id,
                owner.TelegramId,
                owner.FirstName,
                owner.LastName,
                owner.Username,
                owner.Roles);
            
            ownerDtoList.Add(ownerDto);
        }

        return new GetOwnersResponse(ownerDtoList);
    }
}
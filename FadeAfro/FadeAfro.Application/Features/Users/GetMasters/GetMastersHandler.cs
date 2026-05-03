using FadeAfro.Application.Features.Users.Common;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetMasters;

public class GetMastersHandler : IRequestHandler<GetMastersQuery, GetMastersResponse>
{
    private readonly IUserRepository _userRepository;
    
    public GetMastersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<GetMastersResponse> Handle(GetMastersQuery request, CancellationToken cancellationToken)
    {
        var masters = await _userRepository.GetByRoleAsync(Role.Master);

        var masterDtoList = new List<UserDto>();

        foreach (var master in masters)
        {
            var masterDto = new UserDto(
                master.Id,
                master.TelegramId,
                master.FirstName,
                master.LastName,
                master.Username,
                master.Roles);
            
            masterDtoList.Add(masterDto);
        }

        return new GetMastersResponse(masterDtoList);
    }
}
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.GetUser;

public class GetUserHandler : IRequestHandler<GetUserQuery, GetUserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserResponse> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByTelegramIdAsync(query.TelegramId);

        if (user is null)
            throw new UserNotFoundException();

        return new GetUserResponse(
            user.Id,
            user.TelegramId,
            user.FirstName,
            user.LastName,
            user.Username,
            user.Roles);
    }
}

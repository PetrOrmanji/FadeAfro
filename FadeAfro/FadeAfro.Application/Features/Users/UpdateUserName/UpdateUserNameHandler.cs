using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.UpdateUserName;

public class UpdateUserNameHandler : IRequestHandler<UpdateUserNameCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserNameHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpdateUserNameCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);

        if (user is null)
            throw new UserNotFoundException();

        user.UpdateName(command.FirstName, command.LastName);
        await _userRepository.UpdateAsync(user);

        return Unit.Value;
    }
}

using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.UpdateUserName;

public class UpdateUserFullNameHandler : IRequestHandler<UpdateUserFullNameCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserFullNameHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(UpdateUserFullNameCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);

        if (user is null)
            throw new UserNotFoundException();

        user.UpdateFullName(command.FirstName, command.LastName);
        await _userRepository.UpdateAsync(user);
    }
}

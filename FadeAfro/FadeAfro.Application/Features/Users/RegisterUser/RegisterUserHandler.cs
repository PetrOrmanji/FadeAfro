using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Users.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByTelegramIdAsync(command.TelegramId);

        if (existingUser is not null)
            throw new UserAlreadyExistsException();

        var user = new User(
            command.TelegramId,
            command.FirstName,
            command.LastName,
            command.Username,
            [Role.Client]);

        await _userRepository.AddAsync(user);

        return new RegisterUserResponse(user.Id);
    }
}

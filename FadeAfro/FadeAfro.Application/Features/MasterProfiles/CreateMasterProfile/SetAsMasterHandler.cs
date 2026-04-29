using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.CreateMasterProfile;

public class SetAsMasterHandler : IRequestHandler<SetAsMasterCommand, Unit>
{
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IUserRepository _userRepository;

    public SetAsMasterHandler(IMasterProfileRepository masterProfileRepository, IUserRepository userRepository)
    {
        _masterProfileRepository = masterProfileRepository;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(SetAsMasterCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.MasterId);

        if (user is null)
            throw new UserNotFoundException();

        var existingProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);

        if (existingProfile is not null)
            throw new MasterProfileAlreadyExistsException();

        user.AssignMasterRole();
        await _userRepository.UpdateAsync(user);

        var masterProfile = new MasterProfile(
            command.MasterId,
            command.PhotoUrl,
            command.Description);

        await _masterProfileRepository.AddAsync(masterProfile);

        return Unit.Value;
    }
}

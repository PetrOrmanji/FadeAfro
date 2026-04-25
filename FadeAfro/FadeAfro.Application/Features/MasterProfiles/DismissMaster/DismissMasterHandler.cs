using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.DismissMaster;

public class DismissMasterHandler : IRequestHandler<DismissMasterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public DismissMasterHandler(IUserRepository userRepository, IMasterProfileRepository masterProfileRepository)
    {
        _userRepository = userRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(DismissMasterCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);

        if (user is null)
            throw new UserNotFoundException();

        user.RevokeMasterRole();
        await _userRepository.UpdateAsync(user);

        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.UserId);

        if (masterProfile is not null)
            await _masterProfileRepository.DeleteAsync(masterProfile);
    }
}

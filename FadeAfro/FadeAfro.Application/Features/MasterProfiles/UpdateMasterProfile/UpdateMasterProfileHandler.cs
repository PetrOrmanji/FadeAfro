using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfile;

public class UpdateMasterProfileHandler : IRequestHandler<UpdateMasterProfileCommand, Unit>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public UpdateMasterProfileHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<Unit> Handle(UpdateMasterProfileCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        masterProfile.Update(command.PhotoUrl, command.Description);

        await _masterProfileRepository.UpdateAsync(masterProfile);

        return Unit.Value;
    }
}

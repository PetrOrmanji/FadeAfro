using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterDescription;

public class UpdateMasterDescriptionHandler : IRequestHandler<UpdateMasterDescriptionCommand, Unit>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public UpdateMasterDescriptionHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<Unit> Handle(UpdateMasterDescriptionCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        masterProfile.UpdateDescription(command.Description);

        await _masterProfileRepository.UpdateAsync(masterProfile);

        return Unit.Value;
    }
}

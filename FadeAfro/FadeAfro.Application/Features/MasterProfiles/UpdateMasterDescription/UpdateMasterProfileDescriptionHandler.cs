using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterDescription;

public class UpdateMasterProfileDescriptionHandler : IRequestHandler<UpdateMasterProfileDescriptionCommand, Unit>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public UpdateMasterProfileDescriptionHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<Unit> Handle(UpdateMasterProfileDescriptionCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        masterProfile.UpdateDescription(command.Description);

        await _masterProfileRepository.UpdateAsync(masterProfile);

        return Unit.Value;
    }
}

using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterServices.UpdateMasterService;

public class UpdateMasterServiceHandler : IRequestHandler<UpdateMasterServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public UpdateMasterServiceHandler(
        IServiceRepository serviceRepository, 
        IMasterProfileRepository masterProfileRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(UpdateMasterServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile == null)
            throw new MasterProfileNotFoundException();
        
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);
        if (service is null)
            throw new ServiceNotFoundException();

        if (service.MasterProfileId != masterProfile.Id)
            throw new ServiceFromAnotherMasterException();

        service.Update(command.Name, command.Description, command.Price, command.Duration);

        await _serviceRepository.UpdateAsync(service);
    }
}

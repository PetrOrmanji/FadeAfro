using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterServices.DeleteMasterService;

public class DeleteMasterServiceHandler : IRequestHandler<DeleteMasterServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    
    public DeleteMasterServiceHandler(
        IServiceRepository serviceRepository,
        IMasterProfileRepository masterProfileRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(DeleteMasterServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();
        
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);

        if (service is null)
            throw new ServiceNotFoundException();
        
        if (service.MasterProfileId != masterProfile.Id)
            throw new ServiceFromAnotherMasterException();

        await _serviceRepository.DeleteAsync(service);
    }
}

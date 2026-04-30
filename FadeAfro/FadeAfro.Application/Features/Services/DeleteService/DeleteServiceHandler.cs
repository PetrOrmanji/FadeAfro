using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.DeleteService;

public class DeleteServiceHandler : IRequestHandler<DeleteServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    
    public DeleteServiceHandler(
        IServiceRepository serviceRepository,
        IMasterProfileRepository masterProfileRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(DeleteServiceCommand command, CancellationToken cancellationToken)
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

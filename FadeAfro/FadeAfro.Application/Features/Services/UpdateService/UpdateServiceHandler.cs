using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.UpdateService;

public class UpdateServiceHandler : IRequestHandler<UpdateServiceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public UpdateServiceHandler(
        IServiceRepository serviceRepository, 
        IMasterProfileRepository masterProfileRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<Unit> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.UserId);
        if (masterProfile == null)
            throw new MasterProfileNotFoundException();
        
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);
        if (service is null)
            throw new ServiceNotFoundException();

        if (service.MasterProfileId != masterProfile.Id)
            throw new ServiceFromAnotherMasterException();

        service.Update(command.Name, command.Description, command.Price, command.Duration);

        await _serviceRepository.UpdateAsync(service);

        return Unit.Value;
    }
}

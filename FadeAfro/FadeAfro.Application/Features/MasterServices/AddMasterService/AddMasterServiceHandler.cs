using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterServices.AddMasterService;

public class AddMasterServiceHandler : IRequestHandler<AddMasterServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public AddMasterServiceHandler(
        IServiceRepository serviceRepository, 
        IMasterProfileRepository masterProfileRepository,
        IUserRepository userRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(AddMasterServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = new Service(
            masterProfile.Id,
            command.Name,
            command.Description,
            command.Price,
            command.Duration);

        await _serviceRepository.AddAsync(service);
    }
}

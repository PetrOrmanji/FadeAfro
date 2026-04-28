using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.AddService;

public class AddServiceHandler : IRequestHandler<AddServiceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public AddServiceHandler(
        IServiceRepository serviceRepository, 
        IMasterProfileRepository masterProfileRepository,
        IUserRepository userRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<Unit> Handle(AddServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.UserId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = new Service(
            masterProfile.Id,
            command.Name,
            command.Description,
            command.Price,
            command.Duration);

        await _serviceRepository.AddAsync(service);
        
        return Unit.Value;
    }
}

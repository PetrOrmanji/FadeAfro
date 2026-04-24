using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.AddService;

public class AddServiceHandler : IRequestHandler<AddServiceCommand, AddServiceResponse>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public AddServiceHandler(IServiceRepository serviceRepository, IMasterProfileRepository masterProfileRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<AddServiceResponse> Handle(AddServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = new Service(
            command.MasterProfileId,
            command.Name,
            command.Description,
            command.Price,
            command.Duration);

        await _serviceRepository.AddAsync(service);

        return new AddServiceResponse(service.Id);
    }
}

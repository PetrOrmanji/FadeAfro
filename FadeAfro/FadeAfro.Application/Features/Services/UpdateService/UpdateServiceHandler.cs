using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.UpdateService;

public class UpdateServiceHandler : IRequestHandler<UpdateServiceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;

    public UpdateServiceHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Unit> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);

        if (service is null)
            throw new ServiceNotFoundException();

        service.Update(command.Name, command.Description, command.Price, command.Duration);

        await _serviceRepository.UpdateAsync(service);

        return Unit.Value;
    }
}

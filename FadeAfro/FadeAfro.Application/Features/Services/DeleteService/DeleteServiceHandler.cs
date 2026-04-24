using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.DeleteService;

public class DeleteServiceHandler : IRequestHandler<DeleteServiceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;

    public DeleteServiceHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Unit> Handle(DeleteServiceCommand command, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);

        if (service is null)
            throw new ServiceNotFoundException();

        await _serviceRepository.DeleteAsync(command.ServiceId);

        return Unit.Value;
    }
}

using FadeAfro.Application.Features.Services.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Services.GetMasterServices;

public class GetMasterServicesHandler : IRequestHandler<GetMasterServicesQuery, GetMasterServicesResponse>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterServicesHandler(IServiceRepository serviceRepository, IMasterProfileRepository masterProfileRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterServicesResponse> Handle(GetMasterServicesQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var services = await _serviceRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        
        var serviceDtos = new List<ServiceDto>();

        foreach (var service in services)
        {
            var serviceDto = new ServiceDto(
                service.Id,
                service.Name,
                service.Description,
                service.Price,
                service.Duration);
            
            serviceDtos.Add(serviceDto);
        }
        
        return new GetMasterServicesResponse(serviceDtos);
    }
}

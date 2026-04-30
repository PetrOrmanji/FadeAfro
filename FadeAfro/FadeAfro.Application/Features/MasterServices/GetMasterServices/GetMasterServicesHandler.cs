using FadeAfro.Application.Features.MasterServices.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterServices.GetMasterServices;

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

        var masterServices = await _serviceRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        
        var masterServiceDtoList = new List<MasterServiceDto>();

        foreach (var service in masterServices)
        {
            var serviceDto = new MasterServiceDto(
                service.Id,
                service.Name,
                service.Description,
                service.Price,
                service.Duration);
            
            masterServiceDtoList.Add(serviceDto);
        }
        
        return new GetMasterServicesResponse(masterServiceDtoList);
    }
}

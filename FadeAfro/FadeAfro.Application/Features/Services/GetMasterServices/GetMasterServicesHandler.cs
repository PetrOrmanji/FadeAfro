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

        var response = services
            .Select(s => new ServiceResponse(
                s.Id,
                s.Name,
                s.Description,
                s.Price,
                s.Duration))
            .ToList();

        return new GetMasterServicesResponse(response);
    }
}

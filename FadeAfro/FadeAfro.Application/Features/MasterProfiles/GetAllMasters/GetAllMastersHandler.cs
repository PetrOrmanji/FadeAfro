using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasters;

public class GetAllMastersHandler : IRequestHandler<GetAllMastersQuery, GetAllMastersResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetAllMastersHandler(IMasterProfileRepository masterProfileRepository)
    {
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetAllMastersResponse> Handle(GetAllMastersQuery query, CancellationToken cancellationToken)
    {
        var masterProfiles = await _masterProfileRepository.GetAllAsync();

        var masters = masterProfiles
            .Select(mp => new MasterProfileResponse(
                mp.Id,
                mp.MasterId,
                mp.Master.FirstName,
                mp.Master.LastName,
                mp.PhotoUrl,
                mp.Description))
            .ToList();

        return new GetAllMastersResponse(masters);
    }
}

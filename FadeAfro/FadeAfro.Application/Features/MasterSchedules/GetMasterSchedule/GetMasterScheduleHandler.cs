using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;

public class GetMasterScheduleHandler : IRequestHandler<GetMasterScheduleQuery, GetMasterScheduleResponse>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterScheduleHandler(IMasterScheduleRepository masterScheduleRepository, IMasterProfileRepository masterProfileRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterScheduleResponse> Handle(GetMasterScheduleQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var schedules = await _masterScheduleRepository.GetByMasterProfileIdAsync(query.MasterProfileId);

        var response = schedules
            .Select(s => new ScheduleResponse(
                s.Id,
                s.DayOfWeek,
                s.StartTime,
                s.EndTime))
            .ToList();

        return new GetMasterScheduleResponse(response);
    }
}

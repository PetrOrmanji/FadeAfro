using FadeAfro.Application.Features.MasterSchedules.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;

public class GetMasterScheduleHandler : IRequestHandler<GetMasterScheduleQuery, GetMasterScheduleResponse>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterScheduleHandler(
        IMasterScheduleRepository masterScheduleRepository,
        IMasterProfileRepository masterProfileRepository)
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

        var scheduleDtos = new List<ScheduleDto>();

        foreach (var schedule in schedules)
        {
            var scheduleDto = new ScheduleDto(
                schedule.Id,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime);
            
            scheduleDtos.Add(scheduleDto);
        }
       
        return new GetMasterScheduleResponse(scheduleDtos);
    }
}

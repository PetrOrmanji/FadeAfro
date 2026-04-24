using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.SetSchedule;

public class SetScheduleHandler : IRequestHandler<SetScheduleCommand, SetScheduleResponse>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public SetScheduleHandler(IMasterScheduleRepository masterScheduleRepository, IMasterProfileRepository masterProfileRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<SetScheduleResponse> Handle(SetScheduleCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var schedule = new MasterSchedule(
            command.MasterProfileId,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime);

        await _masterScheduleRepository.AddAsync(schedule);

        return new SetScheduleResponse(schedule.Id);
    }
}

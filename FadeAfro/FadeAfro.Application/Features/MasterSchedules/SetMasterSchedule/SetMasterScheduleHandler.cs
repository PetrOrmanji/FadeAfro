using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.SetMasterSchedule;

public class SetMasterScheduleHandler : IRequestHandler<SetMasterScheduleCommand>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public SetMasterScheduleHandler(IMasterScheduleRepository masterScheduleRepository, IMasterProfileRepository masterProfileRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(SetMasterScheduleCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var existing = await _masterScheduleRepository.GetByMasterProfileIdAndDayAsync(
            masterProfile.Id,
            command.DayOfWeek);

        if (existing is not null)
        {
            existing.UpdateTimes(command.StartTime, command.EndTime);
            await _masterScheduleRepository.UpdateAsync(existing);
        }

        var schedule = new MasterSchedule(
            masterProfile.Id,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime);

        await _masterScheduleRepository.AddAsync(schedule);
    }
}

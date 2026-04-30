using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.DeleteMasterSchedule;

public class DeleteMasterScheduleHandler : IRequestHandler<DeleteMasterScheduleCommand>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository  _masterProfileRepository;

    public DeleteMasterScheduleHandler(
        IMasterScheduleRepository masterScheduleRepository,
        IMasterProfileRepository masterProfileRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(DeleteMasterScheduleCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile == null)
            throw new MasterProfileNotFoundException();
        
        var schedule = await _masterScheduleRepository.GetByIdAsync(command.ScheduleId);

        if (schedule is null)
            throw new MasterScheduleNotFoundException();
        
        if (schedule.MasterProfileId != masterProfile.Id)
            throw new ScheduleOfAnotherMasterException();

        await _masterScheduleRepository.DeleteAsync(schedule);
    }
}

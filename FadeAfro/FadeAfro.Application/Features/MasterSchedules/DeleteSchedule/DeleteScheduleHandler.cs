using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.DeleteSchedule;

public class DeleteScheduleHandler : IRequestHandler<DeleteScheduleCommand, Unit>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;

    public DeleteScheduleHandler(IMasterScheduleRepository masterScheduleRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
    }

    public async Task<Unit> Handle(DeleteScheduleCommand command, CancellationToken cancellationToken)
    {
        var schedule = await _masterScheduleRepository.GetByIdAsync(command.ScheduleId);

        if (schedule is null)
            throw new MasterScheduleNotFoundException();

        await _masterScheduleRepository.DeleteAsync(command.ScheduleId);

        return Unit.Value;
    }
}

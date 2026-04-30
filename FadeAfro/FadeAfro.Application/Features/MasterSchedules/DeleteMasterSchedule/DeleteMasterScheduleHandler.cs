using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.DeleteMasterSchedule;

public class DeleteMasterScheduleHandler : IRequestHandler<DeleteMasterScheduleCommand>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository  _masterProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public DeleteMasterScheduleHandler(
        IMasterScheduleRepository masterScheduleRepository,
        IMasterProfileRepository masterProfileRepository,
        IAppointmentRepository appointmentRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
        _masterProfileRepository = masterProfileRepository;
        _appointmentRepository = appointmentRepository;
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

        var scheduleDatesFromToday = GetScheduleDatesFromToday(schedule);
        
        var hasActiveAppointmentsOnDates = 
            await _appointmentRepository.HasActiveAppointmentsOnDatesAsync(scheduleDatesFromToday);

        if (hasActiveAppointmentsOnDates)
            throw new MasterScheduleConflictException(
                "Cannot delete schedule: there are active appointments on scheduled dates.");
        
        await _masterScheduleRepository.DeleteAsync(schedule);
    }

    private List<DateOnly> GetScheduleDatesFromToday(MasterSchedule schedule)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = today.AddMonths(2);

        var dates = Enumerable.Range(0, endDate.DayNumber - today.DayNumber)
            .Select(i => today.AddDays(i))
            .Where(d => d.DayOfWeek == schedule.DayOfWeek)
            .ToList();

        return dates;
    }
}

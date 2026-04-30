using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.SetMasterSchedule;

public class SetMasterScheduleHandler : IRequestHandler<SetMasterScheduleCommand>
{
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public SetMasterScheduleHandler(
        IMasterScheduleRepository masterScheduleRepository, 
        IMasterProfileRepository masterProfileRepository,
        IAppointmentRepository appointmentRepository)
    {
        _masterScheduleRepository = masterScheduleRepository;
        _masterProfileRepository = masterProfileRepository;
        _appointmentRepository = appointmentRepository;
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
            var schedulesDatesFromToday = GetScheduleDatesFromToday(existing);

            var hasActiveAppointmentsOnDates =
                await _appointmentRepository.HasActiveAppointmentsOnDatesAsync(masterProfile.Id, schedulesDatesFromToday);

            if (hasActiveAppointmentsOnDates)
                throw new MasterScheduleConflictException(
                    "Cannot delete schedule: there are active appointments on scheduled dates.");
            
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

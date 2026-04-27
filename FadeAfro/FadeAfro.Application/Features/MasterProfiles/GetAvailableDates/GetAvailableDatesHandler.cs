using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAvailableDates;

public class GetAvailableDatesHandler : IRequestHandler<GetAvailableDatesQuery, GetAvailableDatesResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterUnavailabilityRepository _masterUnavailabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAvailableDatesHandler(
        IMasterProfileRepository masterProfileRepository,
        IServiceRepository serviceRepository,
        IMasterScheduleRepository masterScheduleRepository,
        IMasterUnavailabilityRepository masterUnavailabilityRepository,
        IAppointmentRepository appointmentRepository)
    {
        _masterProfileRepository = masterProfileRepository;
        _serviceRepository = serviceRepository;
        _masterScheduleRepository = masterScheduleRepository;
        _masterUnavailabilityRepository = masterUnavailabilityRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetAvailableDatesResponse> Handle(GetAvailableDatesQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = await _serviceRepository.GetByIdAsync(query.ServiceId);
        if (service is null)
            throw new ServiceNotFoundException();

        // Загружаем всё за месяц одним разом
        var schedules = await _masterScheduleRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var unavailabilities = await _masterUnavailabilityRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var appointments = await _appointmentRepository.GetByMasterProfileIdAsync(query.MasterProfileId);

        var daysInMonth = DateTime.DaysInMonth(query.Year, query.Month);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var availableDates = new List<DateOnly>();

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(query.Year, query.Month, day);

            // Прошедшие дни пропускаем
            if (date < today) continue;

            // Есть ли расписание на этот день недели?
            var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek);
            if (schedule is null) continue;

            // Полная блокировка дня?
            var dateUnavailabilities = unavailabilities.Where(u => u.Date == date).ToList();
            if (dateUnavailabilities.Any(u => u.StartTime is null && u.EndTime is null)) continue;

            // Активные записи на этот день
            var activeAppointments = appointments
                .Where(a => a.Status != AppointmentStatus.CancelledByClient
                         && a.Status != AppointmentStatus.CancelledByMaster
                         && DateOnly.FromDateTime(a.StartTime) == date)
                .ToList();

            // Есть хоть один свободный слот?
            if (HasAnySlot(schedule, service.Duration, dateUnavailabilities, activeAppointments))
                availableDates.Add(date);
        }

        return new GetAvailableDatesResponse(availableDates);
    }

    private static bool HasAnySlot(
        MasterSchedule schedule,
        TimeSpan duration,
        List<MasterUnavailability> unavailabilities,
        List<Appointment> appointments)
    {
        var current = schedule.StartTime;

        while (current.Add(duration) <= schedule.EndTime)
        {
            var slotEnd = current.Add(duration);

            var conflictsUnavail = unavailabilities
                .Where(u => u.StartTime.HasValue && u.EndTime.HasValue)
                .Any(u => u.StartTime!.Value < slotEnd && u.EndTime!.Value > current);

            var conflictsAppt = appointments.Any(a =>
            {
                var apptStart = TimeOnly.FromDateTime(a.StartTime);
                var apptEnd = TimeOnly.FromDateTime(a.EndTime);
                return apptStart < slotEnd && apptEnd > current;
            });

            if (!conflictsUnavail && !conflictsAppt)
                return true;

            current = current.Add(duration);
        }

        return false;
    }
}

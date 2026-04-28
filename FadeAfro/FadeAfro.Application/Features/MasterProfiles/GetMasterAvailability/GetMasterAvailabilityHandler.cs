using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterAvailability;

public class GetMasterAvailabilityHandler : IRequestHandler<GetMasterAvailabilityQuery, GetMasterAvailabilityResponse>
{
    private static readonly TimeSpan SlotStep = TimeSpan.FromMinutes(30);

    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterUnavailabilityRepository _masterUnavailabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetMasterAvailabilityHandler(
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

    public async Task<GetMasterAvailabilityResponse> Handle(GetMasterAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = await _serviceRepository.GetByIdAsync(query.ServiceId);
        if (service is null)
            throw new ServiceNotFoundException();

        // Один раз грузим всё из БД
        var schedules = await _masterScheduleRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var unavailabilities = await _masterUnavailabilityRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var appointments = await _appointmentRepository.GetByMasterProfileIdAsync(query.MasterProfileId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // Текущий месяц + следующий
        var rangeEnd = new DateOnly(today.Year, today.Month, 1).AddMonths(2).AddDays(-1);

        var result = new List<DayAvailability>();

        for (var date = today; date <= rangeEnd; date = date.AddDays(1))
        {
            var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek);
            if (schedule is null) continue;

            var dateUnavailabilities = unavailabilities.Where(u => u.Date == date).ToList();
            if (dateUnavailabilities.Any(u => u.StartTime is null && u.EndTime is null)) continue;

            var activeAppointments = appointments
                .Where(a => a.Status != AppointmentStatus.CancelledByClient
                         && a.Status != AppointmentStatus.CancelledByMaster
                         && DateOnly.FromDateTime(a.StartTime) == date)
                .ToList();

            var slots = GenerateSlots(schedule, service.Duration, dateUnavailabilities, activeAppointments);
            if (slots.Count > 0)
                result.Add(new DayAvailability(date, slots));
        }

        return new GetMasterAvailabilityResponse(result);
    }

    private static List<TimeOnly> GenerateSlots(
        MasterSchedule schedule,
        TimeSpan duration,
        List<MasterUnavailability> unavailabilities,
        List<Appointment> appointments)
    {
        var slots = new List<TimeOnly>();
        var current = schedule.StartTime;

        while (current.Add(duration) <= schedule.EndTime)
        {
            var slotEnd = current.Add(duration);

            var hasUnavailabilityConflict = unavailabilities
                .Where(u => u.StartTime.HasValue && u.EndTime.HasValue)
                .Any(u => u.StartTime!.Value < slotEnd && u.EndTime!.Value > current);

            var hasAppointmentConflict = appointments.Any(a =>
            {
                var apptStart = TimeOnly.FromDateTime(a.StartTime);
                var apptEnd = TimeOnly.FromDateTime(a.EndTime);
                return apptStart < slotEnd && apptEnd > current;
            });

            if (!hasUnavailabilityConflict && !hasAppointmentConflict)
                slots.Add(current);

            current = current.Add(SlotStep);
        }

        return slots;
    }
}

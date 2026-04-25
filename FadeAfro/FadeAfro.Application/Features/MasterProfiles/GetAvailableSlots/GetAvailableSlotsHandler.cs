using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetAvailableSlots;

public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsQuery, GetAvailableSlotsResponse>
{
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterUnavailabilityRepository _masterUnavailabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAvailableSlotsHandler(
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

    public async Task<GetAvailableSlotsResponse> Handle(GetAvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = await _serviceRepository.GetByIdAsync(query.ServiceId);
        if (service is null)
            throw new ServiceNotFoundException();

        var schedules = await _masterScheduleRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == query.Date.DayOfWeek);
        if (schedule is null)
            return new GetAvailableSlotsResponse([]);

        var unavailabilities = await _masterUnavailabilityRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var dateUnavailabilities = unavailabilities.Where(u => u.Date == query.Date).ToList();

        var isFullDayUnavailable = dateUnavailabilities.Any(u => u.StartTime is null && u.EndTime is null);
        if (isFullDayUnavailable)
            return new GetAvailableSlotsResponse([]);

        var allAppointments = await _appointmentRepository.GetByMasterProfileIdAsync(query.MasterProfileId);
        var activeAppointments = allAppointments
            .Where(a => a.Status != AppointmentStatus.CancelledByClient
                     && a.Status != AppointmentStatus.CancelledByMaster
                     && DateOnly.FromDateTime(a.StartTime) == query.Date)
            .ToList();

        var slots = GenerateSlots(schedule, service.Duration, dateUnavailabilities, activeAppointments);

        return new GetAvailableSlotsResponse(slots);
    }

    private static List<TimeSlotResponse> GenerateSlots(
        MasterSchedule schedule,
        TimeSpan duration,
        List<MasterUnavailability> unavailabilities,
        List<Appointment> appointments)
    {
        var slots = new List<TimeSlotResponse>();
        var current = schedule.StartTime;

        while (current.Add(duration) <= schedule.EndTime)
        {
            var slotEnd = current.Add(duration);

            if (!ConflictsWithUnavailabilities(current, slotEnd, unavailabilities) &&
                !ConflictsWithAppointments(current, slotEnd, appointments))
            {
                slots.Add(new TimeSlotResponse(current, slotEnd));
            }

            current = current.Add(duration);
        }

        return slots;
    }

    private static bool ConflictsWithUnavailabilities(TimeOnly slotStart, TimeOnly slotEnd, List<MasterUnavailability> unavailabilities)
    {
        return unavailabilities
            .Where(u => u.StartTime.HasValue && u.EndTime.HasValue)
            .Any(u => u.StartTime!.Value < slotEnd && u.EndTime!.Value > slotStart);
    }

    private static bool ConflictsWithAppointments(TimeOnly slotStart, TimeOnly slotEnd, List<Appointment> appointments)
    {
        return appointments.Any(a =>
        {
            var appointmentStart = TimeOnly.FromDateTime(a.StartTime);
            var appointmentEnd = TimeOnly.FromDateTime(a.EndTime);
            return appointmentStart < slotEnd && appointmentEnd > slotStart;
        });
    }
}

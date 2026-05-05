using FadeAfro.Application.Features.MasterProfiles.Common;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfileDayAvailability;

public class GetMasterProfileDayAvailabilityHandler : IRequestHandler<GetMasterProfileDayAvailabilityQuery, GetMasterProfileDayAvailabilityResponse>
{
    private readonly TimeZoneInfo _timeZone;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterUnavailabilityRepository _masterUnavailabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetMasterProfileDayAvailabilityHandler(
        ITimeSettings timeSettings,
        IMasterProfileRepository masterProfileRepository,
        IMasterScheduleRepository masterScheduleRepository,
        IMasterUnavailabilityRepository masterUnavailabilityRepository,
        IAppointmentRepository appointmentRepository)
    {
        _timeZone = timeSettings.TimeZone;
        _masterProfileRepository = masterProfileRepository;
        _masterScheduleRepository = masterScheduleRepository;
        _masterUnavailabilityRepository = masterUnavailabilityRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetMasterProfileDayAvailabilityResponse> Handle(
        GetMasterProfileDayAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);
        if (masterProfile == null)
            throw new MasterProfileNotFoundException();
        
        var masterSchedule = 
            await _masterScheduleRepository.GetByMasterProfileIdAndDayAsync(masterProfile.Id, query.Date.DayOfWeek);

        if (masterSchedule is null)
            throw new MasterScheduleNotFoundException();

        var unavailability =
            await _masterUnavailabilityRepository.GetByMasterProfileIdAndDateAsync(masterProfile.Id, query.Date);

        if (unavailability is not null)
            return new GetMasterProfileDayAvailabilityResponse([]);

        var appointments =
            await _appointmentRepository.GetActiveByMasterProfileIdAsync(masterProfile.Id);
        
        var availableSlots = GenerateSlots(
            masterSchedule, 
            query.Date, 
            query.ServiceDuration, 
            appointments);
        
        return new GetMasterProfileDayAvailabilityResponse(availableSlots);
    }
    
    private List<MasterProfileDateSlotDto> GenerateSlots(
        MasterSchedule masterSchedule,
        DateOnly date,
        TimeSpan duration,
        IReadOnlyList<Appointment> appointments)
    {
        var slotStep = TimeSpan.FromMinutes(30);
        var current = masterSchedule.StartTime.ToTimeSpan();
        var end = masterSchedule.EndTime.ToTimeSpan();

        var slots = new List<MasterProfileDateSlotDto>();

        while (current + duration <= end)
        {
            var slotStartLocal = date.ToDateTime(TimeOnly.FromTimeSpan(current));
            var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartLocal, _timeZone);
            var slotEndUtc = slotStartUtc + duration;

            var isActive = !appointments.Any(a =>
                a.StartTime < slotEndUtc &&
                a.EndTime > slotStartUtc);
            
            var masterProfileDateSlotDto = new MasterProfileDateSlotDto(
                slotStartUtc, 
                isActive);

            slots.Add(masterProfileDateSlotDto);
            current += slotStep;
        }

        return slots;
    }
}
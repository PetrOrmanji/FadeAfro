namespace FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;

public record ScheduleResponse(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record GetMasterScheduleResponse(List<ScheduleResponse> Schedules);

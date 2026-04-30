namespace FadeAfro.Application.Features.MasterSchedules.SetMasterSchedule;

public record SetMasterScheduleRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
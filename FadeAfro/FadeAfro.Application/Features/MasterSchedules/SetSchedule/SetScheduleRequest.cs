namespace FadeAfro.Application.Features.MasterSchedules.SetSchedule;

public record SetScheduleRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);
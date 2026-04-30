namespace FadeAfro.Application.Features.MasterSchedules.Common;

public record ScheduleDto(
    Guid Id,
    DayOfWeek DayOfWeek, 
    TimeOnly StartTime,
    TimeOnly EndTime);
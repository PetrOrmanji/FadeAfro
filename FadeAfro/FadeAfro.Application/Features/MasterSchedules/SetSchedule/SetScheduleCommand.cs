using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.SetSchedule;

public record SetScheduleCommand(
    Guid MasterId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest;

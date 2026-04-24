using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.SetSchedule;

public record SetScheduleCommand(
    Guid MasterProfileId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest<SetScheduleResponse>;

using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.SetMasterSchedule;

public record SetMasterScheduleCommand(
    Guid MasterId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime) : IRequest;

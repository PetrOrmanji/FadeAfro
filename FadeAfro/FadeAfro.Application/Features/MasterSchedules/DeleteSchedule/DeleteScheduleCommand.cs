using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.DeleteSchedule;

public record DeleteScheduleCommand(
    Guid MasterId,
    Guid ScheduleId) : IRequest<Unit>;

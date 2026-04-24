using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.DeleteSchedule;

public record DeleteScheduleCommand(Guid ScheduleId) : IRequest<Unit>;

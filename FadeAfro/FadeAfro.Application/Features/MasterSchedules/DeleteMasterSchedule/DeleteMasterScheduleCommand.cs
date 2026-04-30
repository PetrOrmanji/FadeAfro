using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.DeleteMasterSchedule;

public record DeleteMasterScheduleCommand(
    Guid MasterId,
    Guid ScheduleId) : IRequest;

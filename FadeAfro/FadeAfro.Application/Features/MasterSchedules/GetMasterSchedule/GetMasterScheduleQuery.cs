using MediatR;

namespace FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;

public record GetMasterScheduleQuery(Guid MasterProfileId) : IRequest<GetMasterScheduleResponse>;

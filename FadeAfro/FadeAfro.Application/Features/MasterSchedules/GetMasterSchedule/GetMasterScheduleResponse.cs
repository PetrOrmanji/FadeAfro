using FadeAfro.Application.Features.MasterSchedules.Common;

namespace FadeAfro.Application.Features.MasterSchedules.GetMasterSchedule;

public record GetMasterScheduleResponse(List<ScheduleDto> Schedules);

using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IMasterScheduleRepository
{
    Task<MasterSchedule?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<MasterSchedule>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task<MasterSchedule?> GetByMasterProfileIdAndDayAsync(Guid masterProfileId, DayOfWeek dayOfWeek);
    Task AddAsync(MasterSchedule schedule);
    Task UpdateAsync(MasterSchedule schedule);
    Task DeleteAsync(Guid id);
    Task DeleteRangeAsync(List<MasterSchedule> schedules);
}

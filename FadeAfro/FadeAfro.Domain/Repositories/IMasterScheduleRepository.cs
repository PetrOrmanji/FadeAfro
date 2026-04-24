using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IMasterScheduleRepository
{
    Task<IReadOnlyList<MasterSchedule>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task AddAsync(MasterSchedule schedule);
    Task UpdateAsync(MasterSchedule schedule);
    Task DeleteAsync(Guid id);
}

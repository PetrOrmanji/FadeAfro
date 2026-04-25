using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IMasterProfileRepository
{
    Task<MasterProfile?> GetByIdAsync(Guid id);
    Task<MasterProfile?> GetByMasterIdAsync(Guid masterId);
    Task<IReadOnlyList<MasterProfile>> GetAllAsync();
    Task AddAsync(MasterProfile masterProfile);
    Task UpdateAsync(MasterProfile masterProfile);
    Task DeleteAsync(MasterProfile masterProfile);
}

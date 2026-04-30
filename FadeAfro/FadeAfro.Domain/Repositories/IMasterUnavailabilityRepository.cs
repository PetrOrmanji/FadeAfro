using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IMasterUnavailabilityRepository
{
    Task<MasterUnavailability?> GetByIdAsync(Guid id);
    Task<MasterUnavailability?> GetByMasterProfileIdAndDateAsync(Guid masterProfileId, DateOnly date);
    Task<IReadOnlyList<MasterUnavailability>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task AddAsync(MasterUnavailability unavailability);
    Task UpdateAsync(MasterUnavailability unavailability);
    Task DeleteAsync(MasterUnavailability unavailability);
    Task DeleteRangeAsync(List<MasterUnavailability> unavailabilities);
}

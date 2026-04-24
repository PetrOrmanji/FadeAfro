using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IMasterUnavailabilityRepository
{
    Task<MasterUnavailability?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<MasterUnavailability>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task AddAsync(MasterUnavailability unavailability);
    Task DeleteAsync(Guid id);
}

using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IMasterUnavailabilityRepository
{
    Task<IReadOnlyList<MasterUnavailability>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task AddAsync(MasterUnavailability unavailability);
    Task DeleteAsync(Guid id);
}

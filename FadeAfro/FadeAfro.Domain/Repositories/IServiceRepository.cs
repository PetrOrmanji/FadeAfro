using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Service>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task AddAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(Service service);
}

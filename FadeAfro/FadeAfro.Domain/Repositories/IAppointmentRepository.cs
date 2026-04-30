using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<Appointment>> GetActiveByClientIdAsync(
        Guid clientId,
        bool includeServices = false,
        bool includeMasterInfo = false);

    Task<IReadOnlyList<Appointment>> GetActiveByMasterProfileIdAsync(
        Guid masterProfileId,
        bool includeServices = false,
        bool includeClientInfo = false);
            
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task UpdateRangeAsync(IEnumerable<Appointment> appointments);
}

using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Appointment>> GetByClientIdAsync(Guid clientId);
    Task<IReadOnlyList<Appointment>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
}

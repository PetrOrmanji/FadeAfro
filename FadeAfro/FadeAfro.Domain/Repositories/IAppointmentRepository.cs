using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Appointment>> GetByMasterProfileIdAsync(Guid masterProfileId);
    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, int page, int pageSize);
    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByMasterProfileIdPagedAsync(Guid masterProfileId, int page, int pageSize);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task UpdateRangeAsync(IEnumerable<Appointment> appointments);
}

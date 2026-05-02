using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(
        Guid id, 
        bool includeMasterInfo = false,
        bool includeClientInfo = false);

    Task<bool> HasActiveAppointmentsOnDateAsync(Guid masterProfileId, DateOnly date);
    
    Task<bool> HasActiveAppointmentsOnDatesAsync(Guid masterProfileId, List<DateOnly> dates);

    Task<bool> HasActiveAppointmentsForServiceAsync(Guid serviceId);
    
    Task<int> GetActiveAppointmentsCountByClientIdAsync(Guid masterProfileId);

    Task<IReadOnlyList<Appointment>> GetActiveByClientIdAsync(
        Guid clientId,
        bool includeServices = false,
        bool includeMasterInfo = false);

    Task<IReadOnlyList<Appointment>> GetActiveByMasterProfileIdAsync(
        Guid masterProfileId,
        bool includeServices = false,
        bool includeClientInfo = false);
            
    Task AddAsync(Appointment appointment);
    Task DeleteAsync(Appointment appointment);
}

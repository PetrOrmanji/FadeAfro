using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(
        Guid id, 
        bool includeMasterInfo = false,
        bool includeClientInfo = false);

    Task<List<Appointment>> GetByDateAsync(
        DateOnly date, 
        bool includeMasterInfo = false, 
        bool includeClientInfo = false);

    Task<bool> HasActiveAppointmentsOnDateAsync(Guid masterProfileId, DateOnly date);
    
    Task<bool> HasActiveAppointmentsOnDatesAsync(Guid masterProfileId, List<DateOnly> dates);

    Task<bool> HasActiveAppointmentsForServiceAsync(Guid serviceId);

    Task<bool> HasConflictingAppointmentAsync(Guid masterProfileId, DateTime startTime, DateTime endTime);
    
    Task<int> GetActiveAppointmentsCountByClientIdAsync(Guid masterProfileId);

    Task<List<Appointment>> GetActiveByClientIdAsync(
        Guid clientId,
        bool includeServices = false,
        bool includeMasterInfo = false);

    Task<List<Appointment>> GetActiveByMasterProfileIdAsync(
        Guid masterProfileId,
        bool includeServices = false,
        bool includeClientInfo = false);
            
    Task AddAsync(Appointment appointment);
    Task DeleteAsync(Appointment appointment);
    Task DeleteRangeAsync(List<Appointment> appointments);
}

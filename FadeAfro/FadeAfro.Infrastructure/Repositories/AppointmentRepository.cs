using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly DatabaseContext _context;

    public AppointmentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(
        Guid id, 
        bool includeMasterInfo = false,
        bool includeClientInfo = false)
    {
        var query = _context.Appointments.AsQueryable();

        if (includeMasterInfo)
            query = query.Include(a => a.MasterProfile).ThenInclude(mp => mp.Master);
        
        if (includeClientInfo)
            query = query.Include(a => a.Client);
        
        return await query.FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<List<Appointment>> GetByDateAsync(DateOnly date, bool includeMasterInfo = false, bool includeClientInfo = false)
    {
        var query = _context.Appointments.AsQueryable();
        
        if (includeMasterInfo)
            query = query.Include(a => a.MasterProfile).ThenInclude(mp => mp.Master);
        
        if (includeClientInfo)
            query = query.Include(a => a.Client);

        return await query.Where(x => DateOnly.FromDateTime(x.StartTime) == date).ToListAsync();
    }

    public async Task<bool> HasActiveAppointmentsOnDateAsync(Guid masterProfileId, DateOnly date)
    {
        return await _context.Appointments.AnyAsync(x => 
            x.MasterProfileId == masterProfileId &&
            DateOnly.FromDateTime(x.StartTime) == date);
    }

    public async Task<bool> HasActiveAppointmentsOnDatesAsync(Guid masterProfileId, List<DateOnly> dates)
    {
        return await _context.Appointments.AnyAsync(x => 
            x.MasterProfileId == masterProfileId &&
            dates.Contains(DateOnly.FromDateTime(x.StartTime)));
    }

    public async Task<bool> HasConflictingAppointmentAsync(Guid masterProfileId, DateTime startTime, DateTime endTime)
    {
        return await _context.Appointments.AnyAsync(a =>
            a.MasterProfileId == masterProfileId &&
            a.StartTime < endTime &&
            a.EndTime > startTime);
    }

    public async Task<bool> HasActiveAppointmentsForServiceAsync(Guid serviceId)
    {
        return await _context.Appointments.AnyAsync(x =>
            x.StartTime > DateTime.UtcNow &&
            x.Services.Any(y => y.ServiceId == serviceId));
    }

    public async Task<int> GetActiveAppointmentsCountByClientIdAsync(Guid clientId)
    {
        return await _context.Appointments.CountAsync(a => 
            a.ClientId == clientId &&
            a.EndTime > DateTime.UtcNow);
    }

    public async Task<List<Appointment>> GetActiveByClientIdAsync(
        Guid clientId,
        bool includeServices = false,
        bool includeMasterInfo = false)
    {
        var query = GetActiveAppointmentsQuery();
        
        if (includeServices)
            query = query.Include(a => a.Services);
        
        if (includeMasterInfo)
            query = query.Include(a => a.MasterProfile).ThenInclude(a => a.Master);
        
        return await query.Where(a => a.ClientId == clientId).ToListAsync();
    }

    public async Task<List<Appointment>> GetActiveByMasterProfileIdAsync(
        Guid masterProfileId,
        bool includeServices = false,
        bool includeClientInfo = false)
    {
        var query = GetActiveAppointmentsQuery();
        
        if (includeServices)
            query = query.Include(a => a.Services);

        if (includeClientInfo)
            query = query.Include(a => a.Client);
        
        return await query.Where(a => a.MasterProfileId == masterProfileId).ToListAsync();
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Appointment appointment)
    {
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(List<Appointment> appointments)
    {
        _context.Appointments.RemoveRange(appointments);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Appointment> GetActiveAppointmentsQuery()
    {
        return _context.Appointments.Where(a => 
            a.EndTime > DateTime.UtcNow);
    }
}

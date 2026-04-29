using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
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

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Appointment>> GetActualByClientIdAsync(
        Guid clientId,
        bool includeServices = false,
        bool includeMasterInfo = false)
    {
        var query = GetActualAppointmentsQuery();
        
        if (includeServices)
            query = query.Include(a => a.Services);
        
        if (includeMasterInfo)
            query = query.Include(a => a.MasterProfile).ThenInclude(a => a.Master);
        
        return await query.Where(a => a.ClientId == clientId).ToListAsync();
    }

    public async Task<IReadOnlyList<Appointment>> GetActualByMasterProfileIdAsync(
        Guid masterProfileId,
        bool includeServices = false,
        bool includeClientInfo = false)
    {
        var query = GetActualAppointmentsQuery();
        
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

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Appointment> appointments)
    {
        _context.Appointments.UpdateRange(appointments);
        await _context.SaveChangesAsync();
    }
    
    private IQueryable<Appointment> GetActualAppointmentsQuery()
    {
        return _context.Appointments.Where(a => 
            a.Status == AppointmentStatus.Confirmed && 
            a.EndTime > DateTime.UtcNow);
    }
}

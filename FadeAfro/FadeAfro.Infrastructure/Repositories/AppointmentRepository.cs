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

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Appointment>> GetByMasterProfileIdAsync(Guid masterProfileId)
    {
        return await _context.Appointments
            .Where(a => a.MasterProfileId == masterProfileId)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, int page, int pageSize)
    {
        var query = _context.Appointments.Where(a => a.ClientId == clientId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByMasterProfileIdPagedAsync(Guid masterProfileId, int page, int pageSize)
    {
        var query = _context.Appointments.Where(a => a.MasterProfileId == masterProfileId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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
}

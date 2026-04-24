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

    public async Task<IReadOnlyList<Appointment>> GetByClientIdAsync(Guid clientId)
    {
        return await _context.Appointments
            .Where(a => a.ClientId == clientId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Appointment>> GetByMasterProfileIdAsync(Guid masterProfileId)
    {
        return await _context.Appointments
            .Where(a => a.MasterProfileId == masterProfileId)
            .ToListAsync();
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
}

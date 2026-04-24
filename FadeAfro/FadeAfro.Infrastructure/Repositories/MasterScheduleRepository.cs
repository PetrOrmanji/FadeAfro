using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class MasterScheduleRepository : IMasterScheduleRepository
{
    private readonly DatabaseContext _context;

    public MasterScheduleRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<MasterSchedule?> GetByIdAsync(Guid id)
    {
        return await _context.MasterSchedules.FirstOrDefaultAsync(ms => ms.Id == id);
    }

    public async Task<IReadOnlyList<MasterSchedule>> GetByMasterProfileIdAsync(Guid masterProfileId)
    {
        return await _context.MasterSchedules
            .Where(ms => ms.MasterProfileId == masterProfileId)
            .ToListAsync();
    }

    public async Task AddAsync(MasterSchedule schedule)
    {
        await _context.MasterSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MasterSchedule schedule)
    {
        _context.MasterSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var schedule = await _context.MasterSchedules.FindAsync(id);
        if (schedule is not null)
        {
            _context.MasterSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}

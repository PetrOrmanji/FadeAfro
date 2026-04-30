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

    public async Task<MasterSchedule?> GetByMasterProfileIdAndDayAsync(Guid masterProfileId, DayOfWeek dayOfWeek)
    {
        return await _context.MasterSchedules
            .FirstOrDefaultAsync(ms => ms.MasterProfileId == masterProfileId && ms.DayOfWeek == dayOfWeek);
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

    public async Task DeleteAsync(MasterSchedule schedule)
    {
        _context.MasterSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteRangeAsync(List<MasterSchedule> schedules)
    {
        if (schedules.Count == 0)
            return;
        
        _context.MasterSchedules.RemoveRange(schedules);
        await _context.SaveChangesAsync();
    }
}

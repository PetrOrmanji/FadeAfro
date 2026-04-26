using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class MasterProfileRepository : IMasterProfileRepository
{
    private readonly DatabaseContext _context;

    public MasterProfileRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<MasterProfile?> GetByIdAsync(Guid id)
    {
        return await _context.MasterProfiles
            .Include(mp => mp.Master)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<MasterProfile?> GetByMasterIdAsync(Guid masterId)
    {
        return await _context.MasterProfiles
            .Include(mp => mp.Master)
            .FirstOrDefaultAsync(mp => mp.MasterId == masterId);
    }

    public async Task<IReadOnlyList<MasterProfile>> GetAllAsync()
    {
        return await _context.MasterProfiles
            .Include(mp => mp.Master)
            .ToListAsync();
    }

    public async Task AddAsync(MasterProfile masterProfile)
    {
        await _context.MasterProfiles.AddAsync(masterProfile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MasterProfile masterProfile)
    {
        _context.MasterProfiles.Update(masterProfile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MasterProfile masterProfile)
    {
        _context.MasterProfiles.Remove(masterProfile);
        await _context.SaveChangesAsync();
    }
}

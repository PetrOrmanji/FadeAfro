using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class MasterUnavailabilityRepository : IMasterUnavailabilityRepository
{
    private readonly DatabaseContext _context;

    public MasterUnavailabilityRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<MasterUnavailability?> GetByIdAsync(Guid id)
    {
        return await _context.MasterUnavailabilities.FirstOrDefaultAsync(mu => mu.Id == id);
    }

    public async Task<MasterUnavailability?> GetByMasterProfileIdAndDateAsync(Guid masterProfileId, DateOnly date)
    {
        return await _context.MasterUnavailabilities
            .Where(mu => mu.MasterProfileId == masterProfileId && mu.Date == date)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<MasterUnavailability>> GetByMasterProfileIdAsync(Guid masterProfileId)
    {
        return await _context.MasterUnavailabilities
            .Where(mu => mu.MasterProfileId == masterProfileId)
            .ToListAsync();
    }

    public async Task AddAsync(MasterUnavailability unavailability)
    {
        await _context.MasterUnavailabilities.AddAsync(unavailability);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MasterUnavailability unavailability)
    {
        _context.MasterUnavailabilities.Update(unavailability);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MasterUnavailability unavailability)
    {
        _context.MasterUnavailabilities.Remove(unavailability);
        await _context.SaveChangesAsync();
    }
}

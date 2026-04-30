using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly DatabaseContext _context;

    public ServiceRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Service?> GetByIdAsync(Guid id)
    {
        return await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<Service>> GetByMasterProfileIdAsync(Guid masterProfileId)
    {
        return await _context.Services
            .Where(s => s.MasterProfileId == masterProfileId)
            .ToListAsync();
    }

    public async Task AddAsync(Service service)
    {
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Service service)
    {
        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
    }
}

using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _context;

    public UserRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IList<User>> GetByRoleAsync(Role role)
    {
        return await _context.Users
            .FromSqlRaw("SELECT * FROM \"Users\" WHERE \"Roles\" LIKE {0}", $"%{role}%")
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    }

    public async Task<IList<User>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(search.ToLower()) ||
                (u.LastName != null && u.LastName.ToLower().Contains(search.ToLower())) ||
                (u.Username != null && u.Username.ToLower().Contains(search.ToLower())));

        return await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(search.ToLower()) ||
                (u.LastName != null && u.LastName.ToLower().Contains(search.ToLower())) ||
                (u.Username != null && u.Username.ToLower().Contains(search.ToLower())));

        return await query.CountAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}

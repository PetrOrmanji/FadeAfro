using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;

namespace FadeAfro.Domain.Repositories;

public interface IUserRepository
{
    Task<IList<User>> GetByRoleAsync(Role role);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByTelegramIdAsync(long telegramId);
    Task<IList<User>> GetAllAsync(int page, int pageSize, string? search);
    Task<int> CountAsync(string? search);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

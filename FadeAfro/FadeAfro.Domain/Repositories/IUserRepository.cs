using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByTelegramIdAsync(long telegramId);
    Task<IReadOnlyList<User>> GetAllAsync(int page, int pageSize);
    Task<int> CountAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

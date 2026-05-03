using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly DatabaseContext _context;

    public NotificationRepository(DatabaseContext context)
    {
        _context = context;
    }
    
    public async Task<int> GetUnreadNotificationsCountByUserId(Guid userId)
    {
        return await _context.Notifications.CountAsync(x => x.UserId == userId && x.IsRead == false);
    }
    
    public async Task<List<Notification>> GetNotificationsByUserId(Guid userId)
    {
        return await _context.Notifications.Where(x => x.UserId == userId).ToListAsync();
    }
    
    public async Task<Notification?> GetNotificationByIdAndUserId(Guid id, Guid userId)
    {
        return await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(List<Notification> notifications)
    {
        _context.Notifications.UpdateRange(notifications);
        await _context.SaveChangesAsync();
    }
}
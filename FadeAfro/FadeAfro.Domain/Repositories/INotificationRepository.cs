using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface INotificationRepository
{
    Task<int> GetUnreadNotificationsCountByUserId(Guid userId);
    Task<List<Notification>> GetUnreadNotificationsByUserId(Guid userId);
    Task<Notification?> GetNotificationByIdAndUserId(Guid id, Guid userId);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task UpdateRangeAsync(List<Notification> notifications);
}
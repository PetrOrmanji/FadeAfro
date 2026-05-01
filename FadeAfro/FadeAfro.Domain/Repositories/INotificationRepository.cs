using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Repositories;

public interface INotificationRepository
{
    Task <IReadOnlyList<Notification>> GetNotificationsByUserId(Guid userId);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task UpdateRangeAsync(List<Notification> notifications);
}
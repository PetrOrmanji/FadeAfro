using FadeAfro.Domain.Exceptions.Notification;

namespace FadeAfro.Domain.Entities;

public class Notification : Entity
{
    public Guid UserId { get; private set; }
    public string Text { get; private set; }
    public bool IsRead { get; private set; }

    public User User { get; private set; } = null!;

    public Notification(Guid userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidNotificationTextException();
        
        UserId = userId;
        Text = text;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
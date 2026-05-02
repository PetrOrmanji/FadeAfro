namespace FadeAfro.Domain.Exceptions.Notification;

public class InvalidNotificationTextException()
    : DomainException("Текст уведомления не может быть пустым.");
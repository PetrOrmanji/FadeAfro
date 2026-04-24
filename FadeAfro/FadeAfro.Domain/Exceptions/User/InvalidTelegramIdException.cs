namespace FadeAfro.Domain.Exceptions.User;

public class InvalidTelegramIdException()
    : DomainException("Telegram ID must be a positive number.");

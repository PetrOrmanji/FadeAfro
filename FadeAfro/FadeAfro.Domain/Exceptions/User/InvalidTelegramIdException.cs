namespace FadeAfro.Domain.Exceptions.User;

public class InvalidTelegramIdException()
    : DomainException("Telegram ID должен быть положительным числом.");

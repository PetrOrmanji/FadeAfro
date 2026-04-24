namespace FadeAfro.Domain.Exceptions;

public class InvalidTelegramIdException()
    : DomainException("Telegram ID must be a positive number.");

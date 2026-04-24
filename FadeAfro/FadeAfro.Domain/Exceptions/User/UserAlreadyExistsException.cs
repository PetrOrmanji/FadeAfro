namespace FadeAfro.Domain.Exceptions.User;

public class UserAlreadyExistsException()
    : DomainException("User with this Telegram ID already exists.");

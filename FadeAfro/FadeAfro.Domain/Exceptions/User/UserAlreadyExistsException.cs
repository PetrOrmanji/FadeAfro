namespace FadeAfro.Domain.Exceptions.User;

public class UserAlreadyExistsException()
    : DomainException("Пользователь с таким Telegram ID уже существует.");

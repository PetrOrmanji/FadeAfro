namespace FadeAfro.Domain.Exceptions.MasterProfile;

public class MasterProfileAlreadyExistsException()
    : DomainException("Профиль мастера для этого пользователя уже существует.");

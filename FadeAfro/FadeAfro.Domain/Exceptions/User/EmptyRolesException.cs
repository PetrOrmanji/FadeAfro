namespace FadeAfro.Domain.Exceptions.User;

public class EmptyRolesException()
    : DomainException("Пользователь должен иметь хотя бы одну роль.");

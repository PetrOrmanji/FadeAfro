namespace FadeAfro.Domain.Exceptions.User;

public class EmptyRolesException()
    : DomainException("User must have at least one role.");

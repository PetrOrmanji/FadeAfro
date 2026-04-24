namespace FadeAfro.Domain.Exceptions;

public class EmptyRolesException()
    : DomainException("User must have at least one role.");

namespace FadeAfro.Domain.Exceptions.Service;

public class InvalidServiceNameException()
    : DomainException("Service name cannot be empty.");

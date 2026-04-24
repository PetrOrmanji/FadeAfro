namespace FadeAfro.Domain.Exceptions.Service;

public class InvalidServiceDurationException()
    : DomainException("Service duration must be greater than zero.");

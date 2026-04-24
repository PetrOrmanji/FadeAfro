namespace FadeAfro.Domain.Exceptions.Service;

public class InvalidServicePriceException()
    : DomainException("Service price must be greater than zero.");

namespace FadeAfro.Domain.Exceptions.MasterService;

public class InvalidMasterServicePriceException()
    : DomainException("Service price must be greater than zero.");

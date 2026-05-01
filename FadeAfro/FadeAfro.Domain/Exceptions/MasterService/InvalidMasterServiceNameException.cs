namespace FadeAfro.Domain.Exceptions.MasterService;

public class InvalidMasterServiceNameException()
    : DomainException("Service name cannot be empty.");

namespace FadeAfro.Domain.Exceptions.MasterService;

public class InvalidMasterServiceDurationException()
    : DomainException("Service duration must be greater than zero.");

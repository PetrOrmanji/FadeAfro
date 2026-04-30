namespace FadeAfro.Domain.Exceptions.MasterUnavailability;

public class MasterUnavailabilityConflictException(string message)
    : DomainException(message); 
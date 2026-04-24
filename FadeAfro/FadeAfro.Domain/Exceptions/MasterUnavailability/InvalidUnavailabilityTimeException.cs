namespace FadeAfro.Domain.Exceptions.MasterUnavailability;

public class InvalidUnavailabilityTimeException()
    : DomainException("End time must be greater than start time.");

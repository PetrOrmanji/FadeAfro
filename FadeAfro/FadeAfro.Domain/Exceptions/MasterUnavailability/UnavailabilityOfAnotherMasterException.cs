namespace FadeAfro.Domain.Exceptions.MasterUnavailability;

public class UnavailabilityOfAnotherMasterException()
    : DomainException("Этот отгул принадлежит другому мастеру.");
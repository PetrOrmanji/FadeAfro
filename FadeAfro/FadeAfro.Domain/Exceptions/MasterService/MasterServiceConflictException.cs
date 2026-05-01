namespace FadeAfro.Domain.Exceptions.MasterService;

public class MasterServiceConflictException(string message)
    : DomainException(message);
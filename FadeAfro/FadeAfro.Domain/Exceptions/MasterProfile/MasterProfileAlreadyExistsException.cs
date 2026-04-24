namespace FadeAfro.Domain.Exceptions.MasterProfile;

public class MasterProfileAlreadyExistsException()
    : DomainException("Master profile for this user already exists.");

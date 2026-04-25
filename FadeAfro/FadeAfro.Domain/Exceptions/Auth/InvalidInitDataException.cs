namespace FadeAfro.Domain.Exceptions.Auth;

public class InvalidInitDataException(string reason)
    : DomainException($"Invalid initData: {reason}");

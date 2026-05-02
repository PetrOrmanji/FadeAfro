namespace FadeAfro.Domain.Exceptions.MasterUnavailability;

public class MasterUnavailabilityNotFoundException()
    : DomainException("Отгул мастера на указанный день не найден.");

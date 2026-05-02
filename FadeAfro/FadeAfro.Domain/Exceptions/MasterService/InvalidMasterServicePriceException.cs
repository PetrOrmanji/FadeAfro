namespace FadeAfro.Domain.Exceptions.MasterService;

public class InvalidMasterServicePriceException()
    : DomainException("Цена услуги должна быть больше нуля.");

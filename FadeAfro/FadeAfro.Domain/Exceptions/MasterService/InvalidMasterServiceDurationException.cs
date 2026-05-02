namespace FadeAfro.Domain.Exceptions.MasterService;

public class InvalidMasterServiceDurationException()
    : DomainException("Длительность услуги не может составлять 0 минут.");

namespace FadeAfro.Domain.Exceptions.MasterService;

public class ServiceFromAnotherMasterException()
    : DomainException("Эта услуга принадлежит другому мастеру.");
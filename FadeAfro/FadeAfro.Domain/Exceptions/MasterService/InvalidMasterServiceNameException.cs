namespace FadeAfro.Domain.Exceptions.MasterService;

public class InvalidMasterServiceNameException()
    : DomainException("Название услуги не может быть пустым.");

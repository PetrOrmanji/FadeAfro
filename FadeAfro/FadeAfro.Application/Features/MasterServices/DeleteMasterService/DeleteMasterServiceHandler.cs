using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterService;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterServices.DeleteMasterService;

public class DeleteMasterServiceHandler : IRequestHandler<DeleteMasterServiceCommand>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    
    public DeleteMasterServiceHandler(
        IServiceRepository serviceRepository,
        IMasterProfileRepository masterProfileRepository,
        IAppointmentRepository appointmentRepository)
    {
        _serviceRepository = serviceRepository;
        _masterProfileRepository = masterProfileRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task Handle(DeleteMasterServiceCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();
        
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);

        if (service is null)
            throw new MasterServiceNotFoundException();
        
        if (service.MasterProfileId != masterProfile.Id)
            throw new ServiceFromAnotherMasterException();

        var hasActiveAppointmentsForService = 
            await _appointmentRepository.HasActiveAppointmentsForServiceAsync(service.Id);
        
        if (hasActiveAppointmentsForService)
            throw new MasterServiceConflictException(
                "Нельзя удалить услугу: есть активные записи.");
            
        await _serviceRepository.DeleteAsync(service);
    }
}

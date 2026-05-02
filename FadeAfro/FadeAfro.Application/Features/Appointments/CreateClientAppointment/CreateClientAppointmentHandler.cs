using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterService;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CreateClientAppointment;

public class CreateClientAppointmentHandler : IRequestHandler<CreateClientAppointmentCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IServiceRepository _serviceRepository;

    public CreateClientAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IMasterProfileRepository masterProfileRepository,
        IServiceRepository serviceRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task Handle(CreateClientAppointmentCommand command, CancellationToken cancellationToken)
    {
        if (command.ServiceIds is null || command.ServiceIds.Count == 0)
            throw new AppointmentMustHaveAtLeastOneServiceException();

        if (command.ServiceIds.Distinct().Count() != command.ServiceIds.Count)
            throw new DuplicateAppointmentServiceException();

        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();
        
        var clientActiveAppointmentsCount = 
            await _appointmentRepository.GetActiveAppointmentsCountByClientIdAsync(command.ClientId);

        if (clientActiveAppointmentsCount >= 3)
            throw new ClientAppointmentLimitExceededException();
            
        var services = new List<Service>();
        foreach (var serviceId in command.ServiceIds)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            
            if (service is null)
                throw new MasterServiceNotFoundException();

            if (service.MasterProfileId != masterProfile.Id)
                throw new ServiceFromAnotherMasterException();
            
            services.Add(service);
        }

        var endTime = services.Aggregate(TimeSpan.Zero, (sum, x) => sum + x.Duration);
        var appointment = new Appointment(
            command.ClientId,
            command.MasterProfileId,
            command.StartTime,
            command.StartTime.Add(endTime),
            command.Comment);

        foreach (var service in services)
            appointment.AddService(service.Id, service.Name, service.Price, service.Duration);

        await _appointmentRepository.AddAsync(appointment);
    }
}

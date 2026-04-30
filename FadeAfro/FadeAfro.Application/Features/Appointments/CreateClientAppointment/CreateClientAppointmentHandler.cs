using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
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
        IUserRepository userRepository,
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

        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var services = new List<Service>();
        foreach (var serviceId in command.ServiceIds)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            
            if (service is null)
                throw new ServiceNotFoundException();

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

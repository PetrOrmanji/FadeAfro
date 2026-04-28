using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CreateAppointment;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, CreateAppointmentResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IServiceRepository _serviceRepository;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository,
        IMasterProfileRepository masterProfileRepository,
        IServiceRepository serviceRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
        _masterProfileRepository = masterProfileRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<CreateAppointmentResponse> Handle(CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        if (command.ServiceIds is null || command.ServiceIds.Count == 0)
            throw new AppointmentMustHaveAtLeastOneServiceException();

        var client = await _userRepository.GetByIdAsync(command.ClientId);
        if (client is null)
            throw new UserNotFoundException();

        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var services = new List<Service>();
        foreach (var serviceId in command.ServiceIds)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service is null)
                throw new ServiceNotFoundException();
            services.Add(service);
        }

        var startTime = DateTime.SpecifyKind(command.StartTime, DateTimeKind.Utc);
        var totalDuration = services.Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
        var endTime = startTime.Add(totalDuration);

        var appointment = new Appointment(
            command.ClientId,
            command.MasterProfileId,
            startTime,
            endTime,
            command.Comment);

        foreach (var service in services)
            appointment.AddService(service.Id, service.Name, service.Price, service.Duration);

        await _appointmentRepository.AddAsync(appointment);

        return new CreateAppointmentResponse(appointment.Id);
    }
}

using FadeAfro.Domain.Entities;
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
        var client = await _userRepository.GetByIdAsync(command.ClientId);
        if (client is null)
            throw new UserNotFoundException();

        var masterProfile = await _masterProfileRepository.GetByIdAsync(command.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var service = await _serviceRepository.GetByIdAsync(command.ServiceId);
        if (service is null)
            throw new ServiceNotFoundException();

        var endTime = command.StartTime.Add(service.Duration);

        var appointment = new Appointment(
            command.ClientId,
            command.MasterProfileId,
            command.ServiceId,
            command.StartTime,
            endTime,
            command.Comment);

        await _appointmentRepository.AddAsync(appointment);

        return new CreateAppointmentResponse(appointment.Id);
    }
}

using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByClient;

public class CancelAppointmentByClientHandler : IRequestHandler<CancelAppointmentByClientCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public CancelAppointmentByClientHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task Handle(CancelAppointmentByClientCommand command, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId);
        
        if (appointment is null)
            throw new AppointmentNotFoundException();

        if (appointment.ClientId != command.ClientId)
            throw new AppointmentOfAnotherClient();
        
        //appointment.CancelByClient();

        await _appointmentRepository.UpdateAsync(appointment);
    }
}

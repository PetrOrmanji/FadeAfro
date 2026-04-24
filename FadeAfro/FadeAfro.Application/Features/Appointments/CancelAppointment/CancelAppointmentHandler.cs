using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointment;

public class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public CancelAppointmentHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Unit> Handle(CancelAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId);

        if (appointment is null)
            throw new AppointmentNotFoundException();

        if (command.CancelledByMaster)
            appointment.CancelByMaster();
        else
            appointment.CancelByClient();

        await _appointmentRepository.UpdateAsync(appointment);

        return Unit.Value;
    }
}

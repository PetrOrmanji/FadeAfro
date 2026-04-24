using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CompleteAppointment;

public class CompleteAppointmentHandler : IRequestHandler<CompleteAppointmentCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public CompleteAppointmentHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Unit> Handle(CompleteAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId);

        if (appointment is null)
            throw new AppointmentNotFoundException();

        appointment.Complete();

        await _appointmentRepository.UpdateAsync(appointment);

        return Unit.Value;
    }
}

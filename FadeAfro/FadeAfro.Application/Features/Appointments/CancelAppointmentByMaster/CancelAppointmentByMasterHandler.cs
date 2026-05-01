using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByMaster;

public class CancelAppointmentByMasterHandler : IRequestHandler<CancelAppointmentByMasterCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public CancelAppointmentByMasterHandler(
        IAppointmentRepository appointmentRepository,
        IMasterProfileRepository masterProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task Handle(CancelAppointmentByMasterCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();
        
        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId);
        
        if (appointment is null)
            throw new AppointmentNotFoundException();

        if (appointment.MasterProfileId != masterProfile.Id)
            throw new AppointmentOfAnotherClient();
        
        //appointment.CancelByMaster();

        await _appointmentRepository.UpdateAsync(appointment);
    }
}
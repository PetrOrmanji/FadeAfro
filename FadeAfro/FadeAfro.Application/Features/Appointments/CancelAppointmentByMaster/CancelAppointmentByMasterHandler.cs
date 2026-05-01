using FadeAfro.Application.Services;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByMaster;

public class CancelAppointmentByMasterHandler : IRequestHandler<CancelAppointmentByMasterCommand>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly INotificationService _notificationService;

    public CancelAppointmentByMasterHandler(
        IAppointmentRepository appointmentRepository,
        IMasterProfileRepository masterProfileRepository,
        INotificationService notificationService)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(CancelAppointmentByMasterCommand command, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.MasterId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();
        
        var appointment = await _appointmentRepository.GetByIdAsync(
            command.AppointmentId,
            includeMasterInfo: true,
            includeClientInfo: true);
        
        if (appointment is null)
            throw new AppointmentNotFoundException();

        if (appointment.MasterProfileId != masterProfile.Id)
            throw new AppointmentOfAnotherClient();
        
        await _appointmentRepository.DeleteAsync(appointment);
        
        await _notificationService.NotifyAsync(
            appointment.MasterProfile.Master.Id, 
            appointment.MasterProfile.Master.TelegramId,
            PrepareNotificationText(appointment));
    }
    
    private string PrepareNotificationText(Appointment appointment)
    {
        var masterName = appointment.MasterProfile.Master.GetFullName();
        var masterInfo = string.IsNullOrWhiteSpace(appointment.MasterProfile.Master.Username)
            ? masterName
            : $"{masterName} (@{appointment.MasterProfile.Master.Username})";

        var notificationText = 
            $"❌Мастер {masterInfo} отменил вашу запись на " +
            $"{appointment.StartTime:dd.MM.yyyy} в {appointment.StartTime:HH:mm}.";
        
        return notificationText;
    }
}
using FadeAfro.Application.Services;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByMaster;

public class CancelAppointmentByMasterHandler : IRequestHandler<CancelAppointmentByMasterCommand>
{
    private readonly TimeZoneInfo _timeZone;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly INotificationService _notificationService;

    public CancelAppointmentByMasterHandler(
        ITimeSettings timeSettings,
        IAppointmentRepository appointmentRepository,
        IMasterProfileRepository masterProfileRepository,
        INotificationService notificationService)
    {
        _timeZone = timeSettings.TimeZone;
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
            throw new AppointmentOfAnotherMaster();

        await _appointmentRepository.DeleteAsync(appointment);

        await _notificationService.NotifyAsync(
            appointment.Client.Id,
            appointment.Client.TelegramId,
            PrepareNotificationText(appointment));
    }
    
    private string PrepareNotificationText(Appointment appointment)
    {
        var masterName = appointment.MasterProfile.Master.GetFullName();
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.StartTime, _timeZone);

        var notificationText =
            $"Мастер {masterName} отменил вашу запись на " +
            $"{localTime:dd.MM.yyyy} в {localTime:HH:mm}.";
        
        return notificationText;
    }
}
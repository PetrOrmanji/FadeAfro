using FadeAfro.Application.Services;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.CancelAppointmentByClient;

public class CancelAppointmentByClientHandler : IRequestHandler<CancelAppointmentByClientCommand>
{
    private readonly TimeZoneInfo _timeZone;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationService _notificationService;

    public CancelAppointmentByClientHandler(
        ITimeSettings timeSettings,
        IAppointmentRepository appointmentRepository,
        INotificationService notificationService)
    {
        _timeZone = timeSettings.TimeZone;
        _appointmentRepository = appointmentRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(CancelAppointmentByClientCommand command, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            command.AppointmentId,
            includeMasterInfo: true,
            includeClientInfo: true);
        
        if (appointment is null)
            throw new AppointmentNotFoundException();

        if (appointment.ClientId != command.ClientId)
            throw new AppointmentOfAnotherClient();
        
        await _appointmentRepository.DeleteAsync(appointment);

        await _notificationService.NotifyAsync(
            appointment.MasterProfile.Master.Id, 
            appointment.MasterProfile.Master.TelegramId,
            PrepareNotificationText(appointment));
    }

    private string PrepareNotificationText(Appointment appointment)
    {
        var clientName = appointment.Client.GetFullName();
        var clientInfo = string.IsNullOrWhiteSpace(appointment.Client.Username)
            ? clientName
            : $"{clientName} (@{appointment.Client.Username})";

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.StartTime, _timeZone);

        var notificationText =
            $"Клиент {clientInfo} отменил запись на " +
            $"{localTime:dd.MM.yyyy} в {localTime:HH:mm}.";
        
        return notificationText;
    }
}

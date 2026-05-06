using FadeAfro.Application.Services;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Repositories;
using FadeAfro.Infrastructure.Options.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FadeAfro.Infrastructure.Workers;

public class AppointmentReminderWorker : ScheduledWorker
{
    private readonly ILogger<AppointmentReminderWorker> _logger;
    private readonly TimeZoneInfo _timeZone;
    private readonly IServiceScopeFactory _scopeFactory;

    public AppointmentReminderWorker(
        ITimeSettings timeSettings,
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderWorker> logger,
        IOptions<AppointmentReminderWorkerOptions> options)
        : base(timeSettings.TimeZone, TimeOnly.Parse(options.Value.RunAt))
    {
        _logger = logger;
        _timeZone = timeSettings.TimeZone;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        const string baseLog = $"[{nameof(AppointmentReminderWorker)}.{nameof(ExecuteJobAsync)}]";
        
        _logger.LogInformation($"{baseLog} Sending appointments reminders started");

        try
        {
            using var scope = _scopeFactory.CreateScope();

            var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
            var today = DateOnly.FromDateTime(nowLocal);
            var tomorrow = today.AddDays(1);

            var todayAppointments = await appointmentRepository.GetByDateAsync(
                today, includeMasterInfo: true, includeClientInfo: true);

            var tomorrowAppointments = await appointmentRepository.GetByDateAsync(
                tomorrow, includeMasterInfo: true, includeClientInfo: true);

            foreach (var appointment in todayAppointments)
                await SendReminderAsync(notificationService, appointment, isToday: true);

            foreach (var appointment in tomorrowAppointments)
                await SendReminderAsync(notificationService, appointment, isToday: false);

            _logger.LogInformation($"{baseLog} Sending appointments finished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{baseLog} Error while sending appointments reminders.");
        }
    }

    private async Task SendReminderAsync(
        INotificationService notificationService,
        Appointment appointment,
        bool isToday)
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.StartTime, _timeZone);
        var timeStr = localTime.ToString("HH:mm");
        var masterName = appointment.MasterProfile.Master.FirstName;

        var text = isToday
            ? $"Напоминаем, что сегодня в {timeStr} вас ждёт мастер {masterName}. Будем рады вас видеть!"
            : $"Напоминаем, что завтра в {timeStr} у вас запись к мастеру {masterName}. Ждём вас!";

        await notificationService.NotifyAsync(
            appointment.Client.Id,
            appointment.Client.TelegramId,
            text);
    }
}

using FadeAfro.Application.Services;
using FadeAfro.Application.Settings;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FadeAfro.Application.Features.MasterProfiles.DismissMaster;

public class DismissMasterHandler : IRequestHandler<DismissMasterCommand>
{
    private readonly TimeZoneInfo _timeZone;
    private readonly ILogger<DismissMasterHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;
    
    public DismissMasterHandler(
        ITimeSettings timeSettings,
        ILogger<DismissMasterHandler> logger,
        IUserRepository userRepository,
        IMasterProfileRepository masterProfileRepository,
        IAppointmentRepository appointmentRepository,
        INotificationService notificationService,
        IFileStorageService fileStorageService)
    {
        _timeZone = timeSettings.TimeZone;
        _logger = logger;
        _userRepository = userRepository;
        _masterProfileRepository = masterProfileRepository;
        _appointmentRepository = appointmentRepository;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
    }

    public async Task Handle(DismissMasterCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);

        if (user is null)
            throw new UserNotFoundException();

        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.UserId);

        if (masterProfile is not null)
        {
            var masterAppointments = 
                await _appointmentRepository.GetActiveByMasterProfileIdAsync(masterProfile.Id, includeClientInfo: true);

            var masterName = user.GetFullName();

            foreach (var appointment in masterAppointments)
            {
                await _appointmentRepository.DeleteAsync(appointment);

                await _notificationService.NotifyAsync(
                    appointment.Client.Id,
                    appointment.Client.TelegramId,
                    PrepareNotificationText(appointment, masterName));
            }
            
            await _masterProfileRepository.DeleteAsync(masterProfile);
            DeleteMasterProfilePhoto(masterProfile.Id);
        }
        
        user.RevokeMasterRole();
        await _userRepository.UpdateAsync(user);
    }

    private void DeleteMasterProfilePhoto(Guid masterProfileId)
    {
        try
        {
            _fileStorageService.DeleteMasterPhoto(masterProfileId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to delete master photo for profile {masterProfileId}");
        }
    }
    
    private string PrepareNotificationText(Appointment appointment, string masterName)
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.StartTime, _timeZone);

        return $"Мастер {masterName} отменил вашу запись на " +
               $"{localTime:dd.MM.yyyy} в {localTime:HH:mm}.";
    }
}

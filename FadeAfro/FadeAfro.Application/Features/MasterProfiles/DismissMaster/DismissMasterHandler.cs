using FadeAfro.Application.Services;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.DismissMaster;

public class DismissMasterHandler : IRequestHandler<DismissMasterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IMasterScheduleRepository _masterScheduleRepository;
    private readonly IMasterUnavailabilityRepository _masterUnavailabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IFileStorageService _fileStorageService;

    public DismissMasterHandler(
        IUserRepository userRepository,
        IMasterProfileRepository masterProfileRepository,
        IMasterScheduleRepository masterScheduleRepository,
        IMasterUnavailabilityRepository masterUnavailabilityRepository,
        IAppointmentRepository appointmentRepository,
        IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _masterProfileRepository = masterProfileRepository;
        _masterScheduleRepository = masterScheduleRepository;
        _masterUnavailabilityRepository = masterUnavailabilityRepository;
        _appointmentRepository = appointmentRepository;
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
            var activeAppointments = (await _appointmentRepository.GetActualByMasterProfileIdAsync(masterProfile.Id))
                .Where(a => a.Status is AppointmentStatus.Confirmed)
                .ToList();

            if (activeAppointments.Any())
            {
                foreach (var appointment in activeAppointments)
                    appointment.CancelByMaster();
                
                await _appointmentRepository.UpdateRangeAsync(activeAppointments);
            }

            var masterSchedules = 
                await _masterScheduleRepository.GetByMasterProfileIdAsync(masterProfile.Id);

            if (masterSchedules.Any())
                await _masterScheduleRepository.DeleteRangeAsync(masterSchedules.ToList());
            
            var masterUnavailabilities =
                await _masterUnavailabilityRepository.GetByMasterProfileIdAsync(command.UserId);

            if (masterUnavailabilities.Any())
                await _masterUnavailabilityRepository.DeleteRangeAsync(masterUnavailabilities.ToList());
            
            await _masterProfileRepository.DeleteAsync(masterProfile);

            _fileStorageService.DeleteMasterPhoto(masterProfile.Id);
        }
        
        user.RevokeMasterRole();
        await _userRepository.UpdateAsync(user);
    }
}

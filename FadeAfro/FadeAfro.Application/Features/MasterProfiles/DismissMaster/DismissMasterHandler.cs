using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.DismissMaster;

public class DismissMasterHandler : IRequestHandler<DismissMasterCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public DismissMasterHandler(
        IUserRepository userRepository,
        IMasterProfileRepository masterProfileRepository,
        IAppointmentRepository appointmentRepository)
    {
        _userRepository = userRepository;
        _masterProfileRepository = masterProfileRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task Handle(DismissMasterCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);

        if (user is null)
            throw new UserNotFoundException();

        user.RevokeMasterRole();
        await _userRepository.UpdateAsync(user);

        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(command.UserId);

        if (masterProfile is not null)
        {
            var activeAppointments = (await _appointmentRepository.GetByMasterProfileIdAsync(masterProfile.Id))
                .Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed)
                .ToList();

            foreach (var appointment in activeAppointments)
                appointment.CancelByMaster();

            if (activeAppointments.Count > 0)
                await _appointmentRepository.UpdateRangeAsync(activeAppointments);

            await _masterProfileRepository.DeleteAsync(masterProfile);
        }
    }
}

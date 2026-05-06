using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.SetAsMaster;

public class SetAsMasterHandler : IRequestHandler<SetAsMasterCommand>
{
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public SetAsMasterHandler(
        IMasterProfileRepository masterProfileRepository,
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository)
    {
        _masterProfileRepository = masterProfileRepository;
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task Handle(SetAsMasterCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);

        if (user is null)
            throw new UserNotFoundException();

        var existingProfile = await _masterProfileRepository.GetByMasterIdAsync(command.UserId);

        if (existingProfile is not null)
            throw new MasterProfileAlreadyExistsException();

        var clientAppointments = 
            await _appointmentRepository.GetActiveByClientIdAsync(user.Id);
        
        if (clientAppointments.Count != 0)
            await _appointmentRepository.DeleteRangeAsync(clientAppointments);

        user.AssignMasterRole();
        await _userRepository.UpdateAsync(user);

        var masterProfile = new MasterProfile(command.UserId);

        await _masterProfileRepository.AddAsync(masterProfile);
    }
}

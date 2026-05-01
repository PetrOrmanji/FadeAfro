using FadeAfro.Application.Services;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FadeAfro.Application.Features.MasterProfiles.DismissMaster;

public class DismissMasterHandler : IRequestHandler<DismissMasterCommand>
{
    private readonly ILogger<DismissMasterHandler> _logger;
    
    private readonly IUserRepository _userRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;
    private readonly IFileStorageService _fileStorageService;

    public DismissMasterHandler(
        ILogger<DismissMasterHandler> logger,
        IUserRepository userRepository,
        IMasterProfileRepository masterProfileRepository,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _masterProfileRepository = masterProfileRepository;
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
}

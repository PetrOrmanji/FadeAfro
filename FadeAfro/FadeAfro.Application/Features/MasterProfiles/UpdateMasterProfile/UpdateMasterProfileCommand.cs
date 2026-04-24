using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfile;

public record UpdateMasterProfileCommand(
    Guid MasterProfileId,
    string? PhotoUrl,
    string? Description) : IRequest<Unit>;

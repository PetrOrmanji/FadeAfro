using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterDescription;

public record UpdateMasterDescriptionCommand(
    Guid MasterProfileId,
    string? Description) : IRequest<Unit>;

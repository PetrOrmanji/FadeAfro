using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterDescription;

public record UpdateMasterProfileDescriptionCommand(
    Guid MasterId,
    string? Description) : IRequest;

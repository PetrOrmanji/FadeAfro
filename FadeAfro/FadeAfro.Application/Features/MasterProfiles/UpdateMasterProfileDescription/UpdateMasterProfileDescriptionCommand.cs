using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfileDescription;

public record UpdateMasterProfileDescriptionCommand(
    Guid MasterId,
    string? Description) : IRequest;

using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.CreateMasterProfile;

public record CreateMasterProfileCommand(
    Guid MasterId,
    string? PhotoUrl,
    string? Description) : IRequest<CreateMasterProfileResponse>;

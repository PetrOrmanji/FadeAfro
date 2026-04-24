namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;

public record GetMasterProfileResponse(
    Guid Id,
    Guid MasterId,
    string? PhotoUrl,
    string? Description);

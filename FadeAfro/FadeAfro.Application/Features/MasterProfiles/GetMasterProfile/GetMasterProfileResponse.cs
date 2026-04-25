namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;

public record GetMasterProfileResponse(
    Guid Id,
    Guid MasterId,
    string FirstName,
    string? LastName,
    string? PhotoUrl,
    string? Description);

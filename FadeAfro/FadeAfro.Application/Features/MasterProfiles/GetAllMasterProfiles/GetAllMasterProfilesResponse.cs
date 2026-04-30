namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasterProfiles;

public record MasterProfileResponse(
    Guid Id,
    Guid MasterId,
    string FirstName,
    string? LastName,
    string? PhotoUrl,
    string? Description);

public record GetAllMasterProfilesResponse(List<MasterProfileResponse> Masters);

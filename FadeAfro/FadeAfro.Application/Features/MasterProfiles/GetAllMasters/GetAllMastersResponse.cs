namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasters;

public record MasterProfileResponse(
    Guid Id,
    Guid MasterId,
    string? PhotoUrl,
    string? Description);

public record GetAllMastersResponse(List<MasterProfileResponse> Masters);

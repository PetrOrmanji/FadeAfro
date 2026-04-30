using FadeAfro.Application.Features.MasterProfiles.Common;

namespace FadeAfro.Application.Features.MasterProfiles.GetAllMasterProfiles;

public record GetAllMasterProfilesResponse(List<MasterProfileDto> Masters);

using FadeAfro.Application.Features.Users.Common;

namespace FadeAfro.Application.Features.Users.GetOwners;

public record GetOwnersResponse(List<UserDto> Owners);
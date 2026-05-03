using FadeAfro.Application.Features.Users.Common;

namespace FadeAfro.Application.Features.Users.GetMasters;

public record GetMastersResponse(List<UserDto> Masters);
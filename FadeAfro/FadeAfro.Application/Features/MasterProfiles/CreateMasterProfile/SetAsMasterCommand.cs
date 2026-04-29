using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.CreateMasterProfile;

public record SetAsMasterCommand(
    Guid MasterId,
    string? PhotoUrl,
    string? Description) : IRequest;

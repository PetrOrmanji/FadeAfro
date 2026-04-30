using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.SetAsMaster;

public record SetAsMasterCommand(
    Guid UserId,
    string? PhotoUrl,
    string? Description) : IRequest;

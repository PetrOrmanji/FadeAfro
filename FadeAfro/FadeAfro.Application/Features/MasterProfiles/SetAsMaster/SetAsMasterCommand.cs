using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.SetAsMaster;

public record SetAsMasterCommand(
    Guid MasterId,
    string? PhotoUrl,
    string? Description) : IRequest;

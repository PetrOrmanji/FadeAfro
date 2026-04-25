using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.DismissMaster;

public record DismissMasterCommand(Guid UserId) : IRequest;

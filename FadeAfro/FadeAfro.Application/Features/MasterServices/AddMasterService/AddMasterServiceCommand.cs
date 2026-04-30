using MediatR;

namespace FadeAfro.Application.Features.MasterServices.AddMasterService;

public record AddMasterServiceCommand(
    Guid MasterId,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration) : IRequest;

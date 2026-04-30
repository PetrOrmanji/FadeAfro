using MediatR;

namespace FadeAfro.Application.Features.MasterServices.UpdateMasterService;

public record UpdateMasterServiceCommand(
    Guid MasterId,
    Guid ServiceId,
    string Name,
    string? Description,
    int Price,
    TimeSpan Duration) : IRequest;

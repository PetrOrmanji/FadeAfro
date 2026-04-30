using MediatR;

namespace FadeAfro.Application.Features.MasterServices.DeleteMasterService;

public record DeleteMasterServiceCommand(
    Guid MasterId,
    Guid ServiceId) : IRequest;

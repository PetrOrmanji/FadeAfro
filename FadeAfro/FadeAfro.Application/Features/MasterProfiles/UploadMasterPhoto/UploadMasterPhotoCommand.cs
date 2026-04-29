using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UploadMasterPhoto;

public record UploadMasterPhotoCommand(
    Guid MasterId,
    Stream FileStream,
    string Extension,
    long FileSize) : IRequest;

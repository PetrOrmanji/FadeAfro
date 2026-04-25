using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UploadMasterPhoto;

public record UploadMasterPhotoCommand(
    Guid MasterProfileId,
    Stream FileStream,
    string Extension,
    long FileSize) : IRequest;

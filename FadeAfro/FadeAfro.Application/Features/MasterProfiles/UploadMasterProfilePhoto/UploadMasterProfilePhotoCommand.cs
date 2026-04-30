using MediatR;

namespace FadeAfro.Application.Features.MasterProfiles.UploadMasterProfilePhoto;

public record UploadMasterProfilePhotoCommand(
    Guid MasterId,
    Stream FileStream,
    string Extension,
    long FileSize) : IRequest;

namespace FadeAfro.Application.Features.MasterProfiles.Common;

public record MasterProfileDto(
    Guid Id,
    string FirstName,
    string? LastName,
    string? PhotoUrl,
    string? Description);
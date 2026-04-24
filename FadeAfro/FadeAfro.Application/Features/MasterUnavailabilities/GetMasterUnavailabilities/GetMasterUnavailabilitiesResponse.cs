namespace FadeAfro.Application.Features.MasterUnavailabilities.GetMasterUnavailabilities;

public record UnavailabilityResponse(
    Guid Id,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime);

public record GetMasterUnavailabilitiesResponse(List<UnavailabilityResponse> Unavailabilities);

namespace FadeAfro.Application.Features.MasterProfiles.GetMasterProfileDayAvailability;

public record GetMasterProfileDayAvailabilityRequest(
    DateOnly Date, 
    TimeSpan ServiceDuration);
namespace FadeAfro.Application.Features.MasterProfiles.GetMasterAvailability;

public record DayAvailability(DateOnly Date, List<TimeOnly> Slots);

public record GetMasterAvailabilityResponse(List<DayAvailability> Days);

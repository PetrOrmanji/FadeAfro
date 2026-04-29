namespace FadeAfro.Application.Features.MasterUnavailabilities.AddUnavailability;

public record AddUnavailabilityRequest(
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime);
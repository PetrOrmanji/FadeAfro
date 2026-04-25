namespace FadeAfro.Application.Features.MasterProfiles.GetAvailableSlots;

public record TimeSlotResponse(TimeOnly Start, TimeOnly End);

public record GetAvailableSlotsResponse(List<TimeSlotResponse> Slots);

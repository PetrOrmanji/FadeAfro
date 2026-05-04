namespace FadeAfro.Extensions;

public static class RateLimitingPolicies
{
    // Anonymous
    public const string AuthLogin              = "auth-login";
    public const string MasterPhoto            = "master-photo";
    public const string MasterAvailability     = "master-availability";

    // Authenticated write — medium priority
    public const string Booking                = "booking";
    public const string PhotoUpload            = "photo-upload";
    public const string OwnerAssignMaster      = "owner-assign-master";
    public const string OwnerDismissMaster     = "owner-dismiss-master";

    // Authenticated write — low priority
    public const string AppointmentCancelClient = "appointment-cancel-client";
    public const string AppointmentCancelMaster = "appointment-cancel-master";
    public const string ScheduleSet            = "schedule-set";
    public const string ScheduleDelete         = "schedule-delete";
    public const string UnavailabilityAdd      = "unavailability-add";
    public const string UnavailabilityDelete   = "unavailability-delete";
    public const string ServiceAdd             = "service-add";
    public const string ServiceUpdate          = "service-update";
    public const string ServiceDelete          = "service-delete";
}

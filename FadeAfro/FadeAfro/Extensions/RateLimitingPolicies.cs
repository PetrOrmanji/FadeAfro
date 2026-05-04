namespace FadeAfro.Extensions;

public static class RateLimitingPolicies
{
    // Anonymous
    public const string AuthLogin = "auth-login";
    public const string MasterPhoto = "master-photo";
    public const string MasterAvailability = "master-availability";

    // Authenticated write
    public const string Booking = "booking";
    public const string PhotoUpload = "photo-upload";
    public const string OwnerAction = "owner-action";
}

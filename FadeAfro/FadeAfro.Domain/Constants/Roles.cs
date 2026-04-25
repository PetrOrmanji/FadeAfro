namespace FadeAfro.Domain.Constants;

public static class Roles
{
    public const string Client = "Client";
    private const string Master = "Master";
    public const string Owner = "Owner";

    public const string MasterOrOwner = $"{Master},{Owner}";
    public const string ClientOrOwner = $"{Client},{Owner}";
}

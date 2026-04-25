using System.ComponentModel.DataAnnotations;

namespace FadeAfro.Infrastructure.Options;

public class JwtOptions
{
    [Required]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 60;
}

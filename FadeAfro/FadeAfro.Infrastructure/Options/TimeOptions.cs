using System.ComponentModel.DataAnnotations;

namespace FadeAfro.Infrastructure.Options;

public class TimeOptions
{
    [Required]
    public string TimeZone { get; init; } = "Europe/Moscow";
}

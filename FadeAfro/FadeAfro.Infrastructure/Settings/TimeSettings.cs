using FadeAfro.Application.Settings;
using FadeAfro.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace FadeAfro.Infrastructure.Settings;

public class TimeSettings : ITimeSettings
{
    public TimeZoneInfo TimeZone { get; }

    public TimeSettings(IOptions<TimeOptions> options)
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);
    }
}

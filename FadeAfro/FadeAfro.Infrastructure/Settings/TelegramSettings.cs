using FadeAfro.Application.Settings;
using FadeAfro.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace FadeAfro.Infrastructure.Settings;

public class TelegramSettings : ITelegramSettings
{
    public string BotToken { get; }
    public bool SkipValidation { get; }

    public TelegramSettings(IOptions<TelegramOptions> options)
    {
        BotToken = options.Value.BotToken;
        SkipValidation = options.Value.SkipValidation;
    }
}

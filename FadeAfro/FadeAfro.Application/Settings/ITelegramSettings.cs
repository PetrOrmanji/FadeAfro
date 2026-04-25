namespace FadeAfro.Application.Settings;

public interface ITelegramSettings
{
    string BotToken { get; }
    bool SkipValidation { get; }
}

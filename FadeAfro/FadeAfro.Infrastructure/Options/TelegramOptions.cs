using System.ComponentModel.DataAnnotations;

namespace FadeAfro.Infrastructure.Options;

public class TelegramOptions
{
    [Required]
    public string BotToken { get; init; } = string.Empty;
}

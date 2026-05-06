using System.ComponentModel.DataAnnotations;

namespace FadeAfro.Infrastructure.Options.Workers;

public class AppointmentReminderWorkerOptions
{
    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "RunAt must be in HH:mm format")]
    public string RunAt { get; init; } = "08:00";
}

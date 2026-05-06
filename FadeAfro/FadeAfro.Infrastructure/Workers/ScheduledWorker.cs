using Microsoft.Extensions.Hosting;

namespace FadeAfro.Infrastructure.Workers;

public abstract class ScheduledWorker : BackgroundService
{
    private readonly TimeZoneInfo _timeZone;
    private readonly TimeOnly _runAt;

    protected ScheduledWorker(TimeZoneInfo timeZone, TimeOnly runAt)
    {
        _timeZone = timeZone;
        _runAt = runAt;
    }
    
    protected abstract Task ExecuteJobAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;
            
            await ExecuteJobAsync(stoppingToken);
        }
    }
    
    private TimeSpan GetDelayUntilNextRun()
    {
        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _timeZone);

        var nextRun = nowLocal.Date.Add(_runAt.ToTimeSpan());
        if (nowLocal >= nextRun)
            nextRun = nextRun.AddDays(1);

        var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextRun, _timeZone);
        return nextRunUtc - nowUtc;
    }
}

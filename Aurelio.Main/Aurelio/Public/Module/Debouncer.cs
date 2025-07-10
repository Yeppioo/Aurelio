using System.Timers;

namespace Aurelio.Public.Module;

public class Debouncer
{
    private readonly Timer _timer;
    public Action Action;

    public Debouncer(Action action, double interval = 1000)
    {
        Action = action;
        _timer = new Timer(interval);
        _timer.Elapsed += OnTimerElapsed!;
        _timer.AutoReset = false;
    }

    public void Trigger()
    {
        _timer.Stop();
        _timer.Start();
    }

    private void OnTimerElapsed(object source, ElapsedEventArgs e)
    {
        _timer.Stop();
        Action?.Invoke();
    }

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
namespace Aurelio.Public.Module.App.Services;

public class LoopGC
{
    public static void BeginLoop()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(10000);
                GC.Collect(2, GCCollectionMode.Aggressive, true);
            }
        });
    }
}
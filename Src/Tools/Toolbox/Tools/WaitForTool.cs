using System.Diagnostics;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class WaitForTool
{
    public static void WaitFor(this Func<bool> action, TimeSpan timeOut, TimeSpan? sleep = null)
    {
        TimeSpan toSleep = sleep ?? TimeSpan.FromMilliseconds(100);

        new CancellationTokenSource(timeOut).Action(x =>
        {
            while (true)
            {
                x.Token.ThrowIfCancellationRequested();
                var isReady = action();
                if (isReady) return;  // Condition met

                Thread.Sleep(toSleep);
            }
        });
    }

    public static async Task WaitFor(this Func<Task<bool>> action, TimeSpan timeOut, TimeSpan? sleep = null)
    {
        TimeSpan toSleep = sleep ?? TimeSpan.FromMilliseconds(100);

        await new CancellationTokenSource(TimeSpan.FromMinutes(1)).Func(async x =>
        {
            while (true)
            {
                x.Token.ThrowIfCancellationRequested();
                var isReady = await action();
                if (isReady) return;  // Condition met

                await Task.Delay(toSleep);
            }
        });
    }

    public static void WaitFor(string processName)
    {
        processName.NotEmpty();
        bool first = true;

        while (Process.GetProcessesByName(processName).Length == 0)
        {
            if (first)
            {
                Console.WriteLine($"Waiting for {processName} to start locally, Cntrl-C to stop...");
                first = false;
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        Thread.Sleep(TimeSpan.FromSeconds(2));
    }
}

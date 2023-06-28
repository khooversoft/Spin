using System.Diagnostics;

namespace SpinClusterApi.Application;

internal static class WaitForSpinSiloTool
{
    public static void WaitForSpinSilo(this IApplicationBuilder app)
    {
        bool first = true;

        while (Process.GetProcessesByName("SpinSilo").Length == 0)
        {
            if (first)
            {
                Console.WriteLine("Waiting for SpinSilo to start locally, Cntrl-C to stop...");
                first = false;
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        Thread.Sleep(TimeSpan.FromSeconds(5));
    }
}

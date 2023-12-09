using System.Diagnostics;

namespace SpinPortal.Application;

public static class WaitForSpinClusterTool
{
    public static void WaitForSpinClusterApi(this IApplicationBuilder app)
    {
        bool first = true;

        while (Process.GetProcessesByName("SpinClusterApi").Length == 0)
        {
            if (first)
            {
                Console.WriteLine("Waiting for SpinClusterApi to start locally, Cntrl-C to stop...");
                first = false;
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        Thread.Sleep(TimeSpan.FromSeconds(2));
    }
}

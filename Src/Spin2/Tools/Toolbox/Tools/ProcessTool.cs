using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Toolbox.Tools;

public static class ProcessTool
{
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

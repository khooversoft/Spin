using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace KGraphCmd.Application;

public static class InputTool
{
    public static string GetUserCommand(CancellationToken token, params string[] args)
    {
        Console.WriteLine();

        while (!token.IsCancellationRequested)
        {
            Console.Write($"Select one: {ForDisplay(args)} > ");

            var input = Console.ReadLine();
            if (input.IsEmpty()) continue;

            var matches = args
                .Select((x, i) => (arg: x, index: i, match: x.StartsWith(input, StringComparison.OrdinalIgnoreCase)))
                .Where(x => x.match)
                .Select(x => x.arg)
                .ToArray();

            if (matches.Length != 1)
            {
                Console.WriteLine("Unknown command");
                continue;
            }

            return matches[0];
        }

        return string.Empty;
    }

    public static async Task WaitForInput(CancellationToken token)
    {
        DateTime mark = getMark();
        bool marked = false;

        while (!token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(true);
                if (marked) Console.WriteLine();
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200));

            if (isPass())
            {
                Console.Write('.');
                mark = getMark();
                marked = true;
            }
        }

        static DateTime getMark() => DateTime.Now.AddSeconds(5);
        bool isPass() => DateTime.Now > mark;
    }

    private static string ForDisplay(string[] args) => args.Select(x => ForDisplay(x)).Join(", ");
    private static string ForDisplay(string arg) => $"[{char.ToUpper(arg[0])}]{arg[1..].ToLower()}";
}

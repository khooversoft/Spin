using Toolbox.Extensions;
using Toolbox.Tools;

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
                .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase))
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

    public static async Task<char> WaitForInput(Func<Task> doWork, CancellationToken token)
    {
        MarkTime mark = new MarkTime(TimeSpan.FromSeconds(5));
        MarkTime work = new MarkTime(TimeSpan.FromMicroseconds(500));

        bool marked = false;

        while (!token.IsCancellationRequested)
        {
            if (work.IsPass())
            {
                await doWork();
            }

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo currentKey = Console.ReadKey(true);
                if (marked) Console.WriteLine();
                return currentKey.KeyChar;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            if (mark.IsPass())
            {
                Console.Write('.');
                marked = true;
            }
        }

        return char.MinValue;
    }

    public static void AppendProperties(this Dictionary<string, string?> data, string label, IReadOnlyDictionary<string, string?> data2)
    {
        data2.ForEach(x => data[$"{label}:{x.Key}"] = x.Value);
    }

    public static string ToLoggingFormat(this IEnumerable<KeyValuePair<string, string?>> data)
    {
        var result = data.NotNull()
            .Select(x => $"{x.Key}={fmt(x.Value)}")
            .Join(Environment.NewLine);

        return result;

        static string fmt(string? value) => value?.Replace("{", "{{").Replace("}", "}}") ?? string.Empty;
    }

    private static string ForDisplay(string[] args) => args.Select(x => ForDisplay(x)).Join(", ");
    private static string ForDisplay(string arg) => $"[{char.ToUpper(arg[0])}]{arg[1..].ToLower()}";
}

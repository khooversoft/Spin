using System.Collections.Immutable;
using Toolbox.LangTools;

namespace Toolbox.Tools;

public readonly struct StructureLineRecord
{
    public StructureLineRecord(string message) => (Message, Args) = (message.NotEmpty(), []);

    public StructureLineRecord(string message, object? obj) => (Message, Args) = (message.NotEmpty(), obj switch
    {
        null => [],
        var v => [v],
    });

    public StructureLineRecord(string message, IEnumerable<object?>? args)
    {
        Message = message.NotEmpty();
        Args = args?.ToArray() ?? [];
    }

    public string Message { get; }

    public object?[] Args { get; }
}


public static class StructureLineRecordExtensions
{
    public static IReadOnlyList<string> GetVariables(this StructureLineRecord subject)
    {
        string message = subject.Message;

        var tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .Add("{{", "}}")
            .AddBlock('{', '}')
            .Parse(message);

        var variables = tokens
            .Where(x => x.TokenType == TokenType.Block)
            .Select(x => x.Value)
            .ToImmutableArray();

        return variables;
    }

    public static string BuildStringFormat(this StructureLineRecord subject)
    {
        var tokens = subject.GetVariables()
            .Select((x, i) => (lookfor: $"{{{x}}}", replace: $"{{{i}}}"));

        var stringFormat = tokens.Aggregate(subject.Message, (a, x) => a.Replace(x.lookfor, x.replace));
        return stringFormat;
    }

    public static string Format(this StructureLineRecord subject)
    {
        var args = subject.Args.ToArray();
        if (args.Length == 0) return subject.Message;

        var stringFormat = subject.BuildStringFormat();
        var result = string.Format(stringFormat, args);

        return result;
    }
}
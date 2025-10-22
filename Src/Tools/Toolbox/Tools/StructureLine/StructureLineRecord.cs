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
    //public static IReadOnlyList<string> GetVariables(this StructureLineRecord subject)
    //{
    //    string message = subject.Message;

    //    var tokens = new StringTokenizer()
    //        .UseCollapseWhitespace()
    //        .Add("{{", "}}")
    //        .AddBlock('{', '}')
    //        .Parse(message);

    //    var variables = tokens
    //        .Where(x => x.TokenType == TokenType.Block)
    //        .Select(x => x.Value)
    //        .ToImmutableArray();

    //    return variables;
    //}

    public static IReadOnlyList<string> GetVariables(this StructureLineRecord subject)
    {
        var message = subject.Message;
        if (string.IsNullOrEmpty(message)) return [];

        ReadOnlySpan<char> span = message.AsSpan();
        int len = span.Length;
        int i = 0;

        System.Collections.Generic.List<string> variables = new();

        while (i < len)
        {
            char ch = span[i];

            // Skip escaped open brace "{{"
            if (ch == '{')
            {
                if (i + 1 < len && span[i + 1] == '{')
                {
                    i += 2; // skip literal "{"
                    continue;
                }

                int start = i + 1;
                int j = start;

                // Find closing '}' for this block
                while (j < len && span[j] != '}') j++;

                if (j < len && span[j] == '}')
                {
                    // Trim whitespace inside the braces
                    int s = start;
                    int e = j - 1;

                    while (s <= e && char.IsWhiteSpace(span[s])) s++;
                    while (e >= s && char.IsWhiteSpace(span[e])) e--;

                    if (s <= e)
                    {
                        var nameSpan = span.Slice(s, e - s + 1);
                        if (!nameSpan.IsEmpty)
                        {
                            variables.Add(new string(nameSpan));
                        }
                    }

                    i = j + 1; // advance past '}'
                    continue;
                }

                // No closing '}', advance one to avoid infinite loop
                i++;
                continue;
            }

            // Skip escaped close brace "}}"
            if (ch == '}' && i + 1 < len && span[i + 1] == '}')
            {
                i += 2; // skip literal "}"
                continue;
            }

            i++;
        }

        return variables.ToImmutableArray();
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
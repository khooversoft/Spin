using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public readonly struct GraphTrace
{
    public string Command { get; init; }
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public double DurationMs { get; init; }
}


public static class GraphTraceTool
{
    public static GraphTrace Create(IGraphInstruction graphInstruction, Option option, TimeSpan duration)
    {
        graphInstruction.NotNull();

        string command = GetCommandDesc(graphInstruction);

        var result = new GraphTrace
        {
            Command = command,
            StatusCode = option.StatusCode,
            Error = option.Error,
            DurationMs = duration.TotalMilliseconds,
        };

        return result;
    }

    public static GraphTrace Create(string graphQuery)
    {
        graphQuery.NotEmpty();

        var result = new GraphTrace
        {
            Command = graphQuery,
            StatusCode = StatusCode.OK,
            Error = null,
            DurationMs = 0,
        };

        return result;
    }

    public static IReadOnlyDictionary<string, string?> GetProperties(this GraphTrace subject) => new Dictionary<string, string?>
    {
        { "$type", subject.GetType().Name },
        { nameof(subject.Command), subject.Command },
        { nameof(subject.StatusCode), subject.StatusCode.ToString() },
        { nameof(subject.Error), subject.Error },
        { nameof(subject.DurationMs), subject.DurationMs.ToString() },
    };

    public static GraphTrace? ToObject(this IReadOnlyList<KeyValuePair<string, string?>> subjects)
    {
        subjects.NotNull();

        if (!getValue($"type", out string? type) || type != typeof(GraphTrace).Name) return null;
        if (!getValue(nameof(GraphTrace.Command), out var command)) return null;
        if (!getValue(nameof(GraphTrace.StatusCode), out var statusCodeString)) return null;
        if (!getValue(nameof(GraphTrace.Error), out var error)) return null;
        if (!getValue(nameof(GraphTrace.DurationMs), out var durationMsString)) return null;

        var result = new GraphTrace
        {
            Command = command.NotEmpty(),
            StatusCode = Enum.Parse<StatusCode>(statusCodeString.NotEmpty()),
            Error = error,
            DurationMs = double.Parse(durationMsString.NotEmpty()),
        };

        return result;

        bool getValue(string key, out string? value)
        {
            value = subjects.FirstOrDefault(x => x.Key == key).Value;
            return value != null;
        }
    }

    private static string GetCommandDesc(IGraphInstruction graphInstruction)
    {
        string command = graphInstruction switch
        {
            GiNode giNode => giNode.GetCommandDesc(),
            GiEdge giEdge => giEdge.GetCommandDesc(),
            GiSelect giSelect => giSelect.GetCommandDesc(),
            GiDelete giDelete => giDelete.GetCommandDesc(),

            _ => throw new UnreachableException(),
        };

        return command;
    }
}
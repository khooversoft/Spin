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
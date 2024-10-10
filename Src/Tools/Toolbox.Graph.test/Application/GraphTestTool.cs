using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Application;

public static class GraphTestTool
{
    public static IReadOnlyList<string> GenerateTestCodeSyntaxTree(this IReadOnlyList<IGraphInstruction> subject)
    {
        var seq = new Sequence<string>();

        foreach (var item in subject)
        {
            var instructions = item switch
            {
                GiNode v => BuildNode(v),
                GiEdge v => BuildEdge(v),
                GiSelect v => BuildSelect(v),
                GiDelete v => BuildDelete(v),
                _ => throw new InvalidOperationException(),
            };

            seq += instructions;
        }

        var formattedLines = HandleIndent(seq);

        var lines = new string[][]
        {
            ["IGraphInstruction[] expected = ["],
            [.. formattedLines],
            ["];"],
        };

        return lines.SelectMany(x => x).ToArray();
    }

    private static IReadOnlyList<string> BuildNode(GiNode node)
    {
        IReadOnlyList<string> tags = CreateTags(node.Tags);

        var lines = new string?[][]
        {
            ["new GiNode"],
            ["{"],
            [$"ChangeType = GiChangeType.{node.ChangeType},"],
            [$"Key = \"{node.Key}\","],
            [.. tags],
            [.. CreateData(node.Data)],
            [.. CreateIndex(node.Indexes)],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> BuildEdge(GiEdge edge)
    {
        IReadOnlyList<string> tags = CreateTags(edge.Tags);

        var lines = new string?[][]
        {
            ["new GiEdge"],
            ["{"],
            [$"ChangeType = GiChangeType.{edge.ChangeType},"],
            [$"From = \"{edge.From}\","],
            [$"To = \"{edge.To}\","],
            [$"Type = \"{edge.Type}\","],
            [.. tags],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> BuildSelect(GiSelect select)
    {
        var buildLines = select.Instructions
            .SelectMany(x => x switch
            {
                GiNodeSelect v => BuildNodeSelect(v),
                GiEdgeSelect v => BuildEdgeSelect(v),
                GiLeftJoin v => BuildLeftJoin(v),
                GiFullJoin v => BuildGiFullJoin(v),
                GiReturnNames v => BuildReturnNames(v),
                _ => throw new InvalidOperationException(),
            });

        var lines = new string?[][]
        {
            ["new GiSelect"],
            ["{"],
            ["Instructions = ["],
            [.. buildLines],
            ["],"],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> BuildDelete(GiDelete select)
    {
        var buildLines = select.Instructions
            .SelectMany(x => x switch
            {
                GiNodeSelect v => BuildNodeSelect(v),
                GiEdgeSelect v => BuildEdgeSelect(v),
                GiLeftJoin v => BuildLeftJoin(v),
                GiFullJoin v => BuildGiFullJoin(v),
                _ => throw new InvalidOperationException(),
            });

        var lines = new string?[][]
        {
            ["new GiDelete"],
            ["{"],
            ["Instructions = ["],
            [.. buildLines],
            ["],"],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> BuildNodeSelect(GiNodeSelect select)
    {
        IReadOnlyList<string> tags = CreateTags(select.Tags);

        var lines = new string?[][]
        {
            ["new GiNodeSelect"],
            ["{"],
            [(select.Key.IsNotEmpty() ? $"Key = \"{select.Key}\"," : null)],
            [.. tags],
            [(select.Alias.IsNotEmpty() ? $"Alias = \"{select.Alias}\"," : null)],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> BuildEdgeSelect(GiEdgeSelect select)
    {
        IReadOnlyList<string> tags = CreateTags(select.Tags);

        var lines = new string?[][]
        {
            ["new GiEdgeSelect"],
            ["{"],
            [(select.From.IsNotEmpty() ? $"From = \"{select.From}\"," : null)],
            [(select.To.IsNotEmpty() ? $"To = \"{select.To}\"," : null)],
            [(select.Type.IsNotEmpty() ? $"Type = \"{select.Type}\"," : null)],
            [.. tags],
            [(select.Alias.IsNotEmpty() ? $"Alias = \"{select.Alias}\"," : null)],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> BuildLeftJoin(GiLeftJoin leftJoin) => ["new GiLeftJoin(),"];

    private static IReadOnlyList<string> BuildGiFullJoin(GiFullJoin leftJoin) => ["new GiFullJoin(),"];

    private static IReadOnlyList<string> CreateTags(IReadOnlyDictionary<string, string?> tags)
    {
        if (tags.Count == 0) return Array.Empty<string>();

        var seq = new Sequence<string>();
        seq += "Tags = new Dictionary<string, string?>";
        seq += "{";

        foreach (var item in tags)
        {
            string tag = item.Value == null ? $"[\"{item.Key}\"] = null," : $"[\"{item.Key}\"] = \"{item.Value}\",";
            seq += tag;
        }

        seq += "},";
        return seq;
    }

    private static IReadOnlyList<string> CreateIndex(IReadOnlySet<string> data)
    {
        if (data.Count == 0) return Array.Empty<string>();

        var seq = new Sequence<string>();
        seq += "Indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)";
        seq += "{";

        foreach (var item in data)
        {
            string tag = $"\"{item}\",";
            seq += tag;
        }

        seq += "},";
        return seq;
    }

    private static IReadOnlyList<string> CreateData(IReadOnlyDictionary<string, string> data)
    {
        if (data.Count == 0) return Array.Empty<string>();

        var seq = new Sequence<string>();
        seq += "Data = new Dictionary<string, string>";
        seq += "{";

        foreach (var item in data)
        {
            string tag = $"[\"{item.Key}\"] = \"{item.Value}\",";
            seq += tag;
        }

        seq += "},";
        return seq;
    }

    private static IReadOnlyList<string> BuildReturnNames(GiReturnNames returnNames)
    {
        var lines = new string?[][]
        {
            ["new GiReturnNames"],
            ["{"],
            ["ReturnNames = [ " + returnNames.ReturnNames.Select(x => $"\"{x}\"").Join(", ") + " ]," ],
            ["},"]
        };

        return lines
            .SelectMany(x => x)
            .Where(x => x.IsNotEmpty())
            .OfType<string>()
            .ToArray();
    }

    private static IReadOnlyList<string> HandleIndent(IReadOnlyList<string> lines)
    {
        int indent = 1;

        var output = new Sequence<string>();

        foreach (var item in lines)
        {
            string line = item.Trim();
            if (line.StartsWith("}") || (line == "]" || line == "],")) indent--;

            output += new string(' ', indent * 4) + line;

            if (line.StartsWith("{") || line.EndsWith("= [")) indent++;
        }

        return output;
    }
}

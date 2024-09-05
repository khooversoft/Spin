using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphDataMapParser
{
    public static Option<GraphDataSource> GetGraphDataLink(Stack<LangNode> stack, string name)
    {
        var getOption = GetDataGroup(stack);
        if (getOption.IsError()) return getOption.ToOptionStatus<GraphDataSource>();

        var dataDict = getOption.Return();

        string schemaValue = dataDict.TryGetValue("schema", out var sv) switch
        {
            true => sv.NotEmpty(),
            false => dataDict.Where(x => x.Value == null).Select(x => x.Value).FirstOrDefault() ?? "json"
        };

        string typeName;
        if (!dataDict.TryGetValue("typeName", out typeName!)) typeName = "default";

        string dataValue = (dataDict.Count == 1 && dataDict.First().Value.IsEmpty()) switch
        {
            true => dataDict.First().Key,
            false => dataDict.TryGetValue("data64", out var dv) ? dv.NotEmpty() : string.Empty
        };

        var data = new GraphDataSource
        {
            Name = name,
            Data64 = dataValue,
        };

        if (!data.Validate(out var r)) return r.ToOptionStatus<GraphDataSource>();
        return data;
    }

    public static Option<ImmutableDictionary<string, string?>> GetDataGroup(Stack<LangNode> stack)
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    if (!dict.TryAdd(langNode.Value, null)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                    break;

                case { SyntaxNode.Name: "lvalue" }:
                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    if (!dict.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "dataGroup", Value: "{" }:
                    break;

                case { SyntaxNode.Name: "dataGroup", Value: "}" }:
                    return dict.ToTags();

                default:
                    return (StatusCode.BadRequest, $"Unknown node, SyntaxNode.Name={langNode.SyntaxNode.Name}");
            }
        }

        return (StatusCode.BadRequest, "No closure on data");
    }
}

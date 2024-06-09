using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphAddCommand
{
    private static FrozenSet<string> _validNames = new string[] { "add", "upsert" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static Option<IGraphQL> Parse(Stack<LangNode> stack, bool upsert = false)
    {
        var list = new List<IGraphQL>();

        if (
            !stack.TryPeek(out var cmd) ||
            cmd.SyntaxNode.Name.IsEmpty() ||
            !_validNames.Contains(cmd.SyntaxNode.Name)
        ) return StatusCode.NotFound;

        stack.Pop();
        bool unique = false;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "node" }:
                    Option<GsNodeAdd> nodeParse = ParseNode(stack, upsert);
                    if (nodeParse.IsError()) return nodeParse.ToOptionStatus<IGraphQL>();
                    return nodeParse.Return();

                case { SyntaxNode.Name: "unique" }:
                    unique = true;
                    continue;

                case { SyntaxNode.Name: "edge" }:
                    Option<GsEdgeAdd> edgeParse = ParseEdge(stack, upsert, unique);
                    if (edgeParse.IsError()) return edgeParse.ToOptionStatus<IGraphQL>();
                    return edgeParse.Return();

                case { SyntaxNode.Name: "select-next" }:
                    break;

                default:
                    return (StatusCode.BadRequest, "Unknown language node");
            }
        }

        return (StatusCode.BadRequest, "Unknown language node");
    }

    private static Option<GsNodeAdd> ParseNode(Stack<LangNode> stack, bool upsert)
    {
        string? key = null;
        var tags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var dataMap = new Dictionary<string, GraphDataSource>(StringComparer.OrdinalIgnoreCase);

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    if (!tags.TryAdd(langNode.Value, null)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                    break;

                case { SyntaxNode.Name: "lvalue" }:
                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    switch (langNode.Value.ToLower())
                    {
                        case "key" when key == null: key = rvalue.Value; break;
                        case "key" when key != null: return (StatusCode.BadRequest, "Key already specified");

                        default:
                            if (!tags.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                            break;
                    }

                    break;

                case { SyntaxNode.Name: "dataName" }:
                    var dataGroupOption = GraphDataMapParser.GetGraphDataLink(stack, langNode.Value);
                    if (dataGroupOption.IsError()) return dataGroupOption.ToOptionStatus<GsNodeAdd>();

                    GraphDataSource data = dataGroupOption.Return();
                    dataMap.Add(langNode.Value, data);
                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (key == null) return (StatusCode.BadRequest, "No key and tags must be specified");

                    return new GsNodeAdd
                    {
                        Key = key,
                        Tags = tags.ToTags(),
                        Upsert = upsert,
                        DataMap = dataMap.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
                    };

                default:
                    return (StatusCode.BadRequest, $"Unknown node, SyntaxNode.Name={langNode.SyntaxNode.Name}");
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }

    private static Option<GsEdgeAdd> ParseEdge(Stack<LangNode> stack, bool upsert, bool unique)
    {
        string? fromKey = null!;
        string? toKey = null!;
        string? edgeType = null!;
        var tags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    if (!tags.TryAdd(langNode.Value, null)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                    break;

                case { SyntaxNode.Name: "lvalue" }:
                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    switch (langNode.Value.ToLower())
                    {
                        case "fromkey" when fromKey == null: fromKey = rvalue.Value; break;
                        case "fromkey" when fromKey != null: return (StatusCode.BadRequest, "FromKey already specified");

                        case "tokey" when toKey == null: toKey = rvalue.Value; break;
                        case "tokey" when toKey != null: return (StatusCode.BadRequest, "ToKey already specified");

                        case "edgetype" when edgeType == null: edgeType = rvalue.Value; break;
                        case "edgetype" when edgeType != null: return (StatusCode.BadRequest, "EdgeType already specified");

                        case "link":
                            return (StatusCode.BadRequest, "Link not allowed");

                        default:
                            if (!tags.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                            break;
                    }

                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (fromKey == null || toKey == null) return (StatusCode.BadRequest, "No fromKey, toKey are required");

                    return new GsEdgeAdd
                    {
                        FromKey = fromKey,
                        ToKey = toKey,
                        EdgeType = edgeType,
                        Tags = tags.ToTags(),
                        Upsert = upsert,
                        Unique = unique,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }
}

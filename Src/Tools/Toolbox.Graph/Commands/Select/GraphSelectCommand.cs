using System.Collections.Immutable;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphSelectCommand
{
    public static Option<GsSelect> Parse(Stack<LangNode> stack, string? command = null)
    {
        var list = new List<IGraphQL>();

        if (command != null)
        {
            if (!stack.TryPeek(out var cmd) || cmd.SyntaxNode.Name != command) return (StatusCode.NotFound, $"Command {command} not found");
            stack.Pop();
        }

        ImmutableHashSet<string> gsReturn = ImmutableHashSet<string>.Empty;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "node-group" }:
                    Option<GraphNodeSearch> nodeParse = ParseNode(stack);
                    if (nodeParse.IsError()) return nodeParse.ToOptionStatus<GsSelect>();
                    list.Add(nodeParse.Return());
                    break;

                case { SyntaxNode.Name: "edge-group" }:
                    Option<GraphEdgeSearch> edgeParse = ParseEdge(stack);
                    if (edgeParse.IsError()) return edgeParse.ToOptionStatus<GsSelect>();
                    list.Add(edgeParse.Return());
                    break;

                case { SyntaxNode.Name: "select-next" }:
                    break;


                case { SyntaxNode.Name: "return" }:
                    var returnParse = GraphReturnParser.ParseReturnNames(stack);
                    if (returnParse.IsError()) return returnParse.ToOptionStatus<GsSelect>();
                    gsReturn = returnParse.Return();
                    break;

                case { SyntaxNode.Name: "term" }:
                    return new GsSelect
                    {
                        Search = list.ToImmutableArray(),
                        ReturnNames = gsReturn,
                    };

                case { SyntaxNode.Name: "update-set" }:
                    stack.Push(langNode);

                    return new GsSelect
                    {
                        Search = list.ToImmutableArray(),
                        ReturnNames = gsReturn,
                    };

                default:
                    throw new ArgumentException("Unknown language node");
            }
        }

        return (StatusCode.BadRequest, "No terminate");
    }

    private static Option<GraphNodeSearch> ParseNode(Stack<LangNode> stack)
    {
        string? key = null;
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
                        case "key" when key == null: key = rvalue.Value; break;
                        case "key" when key != null: return (StatusCode.BadRequest, "Key already specified");

                        default:
                            if (!tags.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                            break;
                    }

                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "node-group" }:
                    if (key == null && tags == null) return (StatusCode.BadRequest, "No key or tags specified");

                    string? alias = null;
                    if (stack.TryPeek(out langNode) && langNode.SyntaxNode.Name == "alias")
                    {
                        stack.Pop();
                        alias = langNode.Value;
                    }

                    return new GraphNodeSearch
                    {
                        Key = key,
                        Tags = tags.RemoveCommands().ToTags(),
                        Alias = alias,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }

    private static Option<GraphEdgeSearch> ParseEdge(Stack<LangNode> stack)
    {
        string? nodeKey = null!;
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
                        case "nodekey" when nodeKey == null: nodeKey = rvalue.Value; break;
                        case "nodekey" when nodeKey != null: return (StatusCode.BadRequest, "NodeKey already specified");

                        case "fromkey" when fromKey == null: fromKey = rvalue.Value; break;
                        case "fromkey" when fromKey != null: return (StatusCode.BadRequest, "FromKey already specified");

                        case "tokey" when toKey == null: toKey = rvalue.Value; break;
                        case "tokey" when toKey != null: return (StatusCode.BadRequest, "ToKey already specified");

                        case "edgetype" when edgeType == null: edgeType = rvalue.Value; break;
                        case "edgetype" when edgeType != null: return (StatusCode.BadRequest, "EdgeType already specified");

                        default:
                            if (!tags.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
                            break;
                    }
                    break;

                case { SyntaxNode.Name: "params-delimiter" }:
                    break;

                case { SyntaxNode.Name: "edge-group" }:
                    if (nodeKey == null &&
                        fromKey == null &&
                        toKey == null &&
                        edgeType == null &&
                        tags == null) return (StatusCode.BadRequest, "No parameters specified");

                    string? alias = null;
                    if (stack.TryPeek(out langNode) && langNode.SyntaxNode.Name == "alias")
                    {
                        stack.Pop();
                        alias = langNode.Value;
                    }

                    return new GraphEdgeSearch
                    {
                        NodeKey = nodeKey,
                        FromKey = fromKey,
                        ToKey = toKey,
                        EdgeType = edgeType,
                        Tags = tags.RemoveCommands().ToTags(),
                        Alias = alias,
                        Direction = EdgeDirection.Directed,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }
}

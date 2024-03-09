using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
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
            !_validNames.Contains(cmd.SyntaxNode.Name))
            return StatusCode.NotFound;

        stack.Pop();
        bool unique = false;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "node" }:
                    Option<GraphNodeAdd> nodeParse = ParseNode(stack, upsert);
                    if (nodeParse.IsError()) return nodeParse.ToOptionStatus<IGraphQL>();
                    return nodeParse.Return();

                case { SyntaxNode.Name: "unique" }:
                    unique = true;
                    continue;

                case { SyntaxNode.Name: "edge" }:
                    Option<GraphEdgeAdd> edgeParse = ParseEdge(stack, upsert, unique);
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

    private static Option<GraphNodeAdd> ParseNode(Stack<LangNode> stack, bool upsert)
    {
        string? key = null;
        var tags = new Tags();

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    tags.Add(langNode.Value, null);
                    break;

                case { SyntaxNode.Name: "lvalue" }:
                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    switch (langNode.Value.ToLower())
                    {
                        case "key" when key == null: key = rvalue.Value; break;
                        case "key" when key != null: return (StatusCode.BadRequest, "Key already specified");

                        default:
                            tags.Set(langNode.Value, rvalue.Value);
                            break;
                    }

                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (key == null) return (StatusCode.BadRequest, "No key and tags must be specified");

                    return new GraphNodeAdd
                    {
                        Key = key,
                        Tags = tags,
                        Upsert = upsert,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }

    private static Option<GraphEdgeAdd> ParseEdge(Stack<LangNode> stack, bool upsert, bool unique)
    {
        string? fromKey = null!;
        string? toKey = null!;
        string? edgeType = null!;
        var tags = new Tags();

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    tags.Add(langNode.Value, null);
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

                        default:
                            tags.Set(langNode.Value, rvalue.Value);
                            break;
                    }

                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (fromKey == null || toKey == null) return (StatusCode.BadRequest, "No fromKey, toKey are required");

                    return new GraphEdgeAdd
                    {
                        FromKey = fromKey,
                        ToKey = toKey,
                        EdgeType = edgeType,
                        Tags = tags,
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

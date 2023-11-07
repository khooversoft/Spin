using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class GraphAdd
{
    public static Option<IReadOnlyList<IGraphQL>> Parse(Stack<LangNode> stack)
    {
        var list = new List<IGraphQL>();

        if (!stack.TryPeek(out var cmd) || cmd.SyntaxNode.Name != "add") return StatusCode.NotFound;
        stack.Pop();

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "node" }:
                    Option<GraphNodeAdd> nodeParse = ParseNode(stack);
                    if (nodeParse.IsError()) return nodeParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();
                    list.Add(nodeParse.Return());
                    return list;

                case { SyntaxNode.Name: "edge" }:
                    Option<GraphEdgeAdd> edgeParse = ParseEdge(stack);
                    if (edgeParse.IsError()) return edgeParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();
                    list.Add(edgeParse.Return());
                    return list;

                case { SyntaxNode.Name: "select-next" }:
                    break;

                default:
                    return (StatusCode.BadRequest, "Unknown language node");
            }
        }

        return list;
    }

    private static Option<GraphNodeAdd> ParseNode(Stack<LangNode> stack)
    {
        string? key = null;
        string? tags = null;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "lvalue" }:
                    string lvalue = langNode.Value.ToLower();

                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    switch (lvalue)
                    {
                        case "key" when key == null: key = rvalue.Value; break;
                        case "key" when key != null: return (StatusCode.BadRequest, "Key already specified");

                        case "tags" when tags == null: tags = rvalue.Value; break;
                        case "tags" when tags != null: return (StatusCode.BadRequest, "Tags already specified");

                        default:
                            return (StatusCode.BadRequest, $"Only 'Key' and 'Tags' is valid in lvalue");
                    }

                    break;

                case { SyntaxNode.Name: "value-delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (key == null || tags == null) return (StatusCode.BadRequest, "No key and tags must be specified");

                    return new GraphNodeAdd
                    {
                        Key = key,
                        Tags = tags,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }

    private static Option<GraphEdgeAdd> ParseEdge(Stack<LangNode> stack)
    {
        string? fromKey = null!;
        string? toKey = null!;
        string? edgeType = null!;
        string? tags = null!;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "lvalue" }:
                    string lvalue = langNode.Value.ToLower();

                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    switch (lvalue)
                    {
                        case "fromkey" when fromKey == null: fromKey = rvalue.Value; break;
                        case "fromkey" when fromKey != null: return (StatusCode.BadRequest, "FromKey already specified");

                        case "tokey" when toKey == null: toKey = rvalue.Value; break;
                        case "tokey" when toKey != null: return (StatusCode.BadRequest, "ToKey already specified");

                        case "edgetype" when edgeType == null: edgeType = rvalue.Value; break;
                        case "edgetype" when edgeType != null: return (StatusCode.BadRequest, "EdgeType already specified");

                        case "tags" when tags == null: tags = rvalue.Value; break;
                        case "tags" when tags != null: return (StatusCode.BadRequest, "Tags already specified");

                        default:
                            return (StatusCode.BadRequest, $"Only 'FromKey', 'ToKey', 'EdgeType', and/or 'Tags' is valid in lvalue");
                    }

                    break;

                case { SyntaxNode.Name: "value-delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (fromKey == null || toKey == null) return (StatusCode.BadRequest, "No fromKey, toKey are required");

                    return new GraphEdgeAdd
                    {
                        FromKey = fromKey,
                        ToKey = toKey,
                        EdgeType = edgeType,
                        Tags = tags,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }
}

using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class GraphUpdateCommand
{
    public static Option<IGraphQL> Parse(Stack<LangNode> stack)
    {
        var selectListOption = GraphSelectCommand.Parse(stack, "update");
        if (selectListOption.IsError()) return selectListOption.ToOptionStatus<IGraphQL>();

        IReadOnlyList<IGraphQL> selectList = selectListOption.Return();
        if (selectList.Count == 0) return (StatusCode.BadRequest, "No search for delete");

        if (!stack.TryPop(out var setCommand) || setCommand.SyntaxNode.Name != "update-set") return (StatusCode.BadRequest, "'set' command required");

        switch (selectList.Last())
        {
            case GraphNodeSearch:
                var nodeParse = ParseNode(stack);
                if (nodeParse.IsOk())
                {
                    return nodeParse.Return() with
                    {
                        Search = selectListOption.Return(),
                    };
                }
                break;

            case GraphEdgeSearch:
                var edgeParse = ParseEdge(stack);
                if (edgeParse.IsOk())
                {
                    return edgeParse.Return() with
                    {
                        Search = selectListOption.Return(),
                    };
                }
                break;

            case object v: throw new UnreachableException($"Unknown search type {v.GetType().FullName}");
        }

        return (StatusCode.BadRequest, "No language nodes");
    }

    private static Option<GraphNodeUpdate> ParseNode(Stack<LangNode> stack)
    {
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
                        case "tags" when tags == null: tags = rvalue.Value; break;
                        case "tags" when tags != null: return (StatusCode.BadRequest, "Tags already specified");

                        default:
                            return (StatusCode.BadRequest, $"Only 'Tags' is can be updated");
                    }

                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (tags == null) return (StatusCode.BadRequest, "No key and tags must be specified");

                    return new GraphNodeUpdate
                    {
                        Tags = tags,
                    };

                default: throw new UnreachableException($"Unknown langNode={langNode.GetType().FullName}");
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }

    private static Option<GraphEdgeUpdate> ParseEdge(Stack<LangNode> stack)
    {
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
                        case "edgetype" when edgeType == null: edgeType = rvalue.Value; break;
                        case "edgetype" when edgeType != null: return (StatusCode.BadRequest, "EdgeType already specified");

                        case "tags" when tags == null: tags = rvalue.Value; break;
                        case "tags" when tags != null: return (StatusCode.BadRequest, "Tags already specified");

                        default:
                            return (StatusCode.BadRequest, $"Only 'EdgeType', and/or 'Tags' is valid in lvalue");
                    }

                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (edgeType == null && tags == null) return (StatusCode.BadRequest, "No edgeType, tags are required");

                    return new GraphEdgeUpdate
                    {
                        EdgeType = edgeType,
                        Tags = tags,
                    };

                default: throw new UnreachableException($"Unknown langNode={langNode.GetType().FullName}");
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }
}
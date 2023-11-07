﻿using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class GraphUpdate
{
    public static Option<IReadOnlyList<IGraphQL>> Parse(Stack<LangNode> stack)
    {
        var selectListOption = GraphSelect.Parse(stack, "update");
        if (selectListOption.IsError()) return selectListOption;

        var list = new Sequence<IGraphQL>();

        if (!stack.TryPop(out var setCommand) || setCommand.SyntaxNode.Name != "update-set") return (StatusCode.BadRequest, "Syntax error: Command 'set' is required");

        while (stack.TryPeek(out var langNode))
        {
            var saveStack = new Stack<LangNode>(stack);

            var nodeParse = ParseNode(stack);
            if (nodeParse.IsOk())
            {
                GraphNodeUpdate updateNode = nodeParse.Return() with
                {
                    Search = selectListOption.Return(),
                };

                list += updateNode;
                return list;
            }

            int restoreCount = saveStack.Count - stack.Count;
            Enumerable.Range(0, saveStack.Count - restoreCount).ForEach(_ => saveStack.Pop());
            Enumerable.Range(0, restoreCount).ForEach(_ => stack.Push(saveStack.Pop()));

            var edgeParse = ParseEdge(stack);
            if (edgeParse.IsOk())
            {
                GraphEdgeUpdate updateEdge = edgeParse.Return() with
                {
                    Search = selectListOption.Return(),
                };
                list += updateEdge;
                return list;
            }

            return (StatusCode.BadRequest, "Unknown language node");
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

                case { SyntaxNode.Name: "value-delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (tags == null) return (StatusCode.BadRequest, "No key and tags must be specified");

                    return new GraphNodeUpdate
                    {
                        Tags = tags,
                    };

                default:
                    break;
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

                case { SyntaxNode.Name: "value-delimiter" }:
                    break;

                case { SyntaxNode.Name: "term" }:
                    if (edgeType == null || tags == null) return (StatusCode.BadRequest, "No edgeType, tags are required");

                    return new GraphEdgeUpdate
                    {
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
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IGraphQL
{
}


/// <summary>
/// (key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
/// (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
/// (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule=tagValue) n2
/// 
/// Node properties = Key, Tags
/// Edge properties = NodeKey, FromKey, ToKey, EdgeType, Tags
/// 
/// </summary>
public static class GraphLang
{
    private readonly static LsRoot _equalValue = new LsRoot("equalValue")
        + new LsValue("lvalue")
        + ("=", "equal")
        + new LsValue("rvalue");

    private readonly static LsRoot _valueOnly = new LsRoot("valueOnly") + new LsValue("svalue");

    private readonly static LsRepeat _parameters = new LsRepeat("params-rpt")
        + (new LsSwitch("params-or") + _equalValue + _valueOnly)
        + new LsToken(";", "params-delimiter", true);

    private readonly static LsRoot _nodeSyntax = new LsRoot("node")
        + (new LsGroup("(", ")", "node-group") + _parameters)
        + new LsValue("alias", true);

    private readonly static LsRoot _edgeSyntax = new LsRoot("edge")
        + (new LsGroup("[", "]", "edge-group") + _parameters)
        + new LsValue("alias", true);

    //private readonly static LsRoot _opr = new LsRoot("opr")
    //    + (new LsOr("opr-or")
    //        + (new LsRoot() + new LsToken("->", "next"))
    //        + (new LsRoot() + new LsToken("=", "next"))
    //    );

    private readonly static LsRoot _root = new LsRoot()
        + (new LsRepeat("root-rpt")
            + (new LsSwitch("instr-or") + _nodeSyntax + _edgeSyntax) + new LsToken("->", "next", true)
        );

    public static Option<IReadOnlyList<IGraphQL>> Parse(string rawData)
    {
        LangResult langResult = _root.Parse(rawData);
        if (langResult.IsError()) return new Option<IReadOnlyList<IGraphQL>>(langResult.StatusCode, langResult.Error);

        var stack = langResult.LangNodes.NotNull().Reverse().ToStack();
        var list = new List<IGraphQL>();

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "node-group" }:
                    Option<GraphNodeQuery> nodeParse = ParseNode(stack, list);
                    if (nodeParse.IsError()) return nodeParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();
                    list.Add(nodeParse.Return());
                    break;

                case { SyntaxNode.Name: "edge-group" }:
                    Option<GraphEdgeQuery> edgeParse = ParseEdge(stack, list);
                    if (edgeParse.IsError()) return edgeParse.ToOptionStatus<IReadOnlyList<IGraphQL>>();
                    list.Add(edgeParse.Return());
                    break;

                case { SyntaxNode.Name: "next" }:
                    break;

                default:
                    return (StatusCode.BadRequest, "Unknown language node");
            }
        }

        return list;
    }

    private static Option<GraphNodeQuery> ParseNode(Stack<LangNode> stack, List<IGraphQL> list)
    {
        string? key = null;
        string? tags = null;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    if (tags != null) return (StatusCode.BadRequest, "Tags already specified");
                    tags = langNode.Value;
                    break;

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
                            return (StatusCode.BadRequest, $"Only 'Key' and/or 'Tags' is valid in lvalue");
                    }

                    break;

                case { SyntaxNode.Name: "params-delimiter" }:
                    break;

                case { SyntaxNode.Name: "node-group" }:
                    if (key == null && tags == null) return (StatusCode.BadRequest, "No key or tags specified");

                    string? alias = null;
                    if (stack.TryPeek(out langNode) && langNode.SyntaxNode.Name == "alias")
                    {
                        stack.Pop();
                        alias = langNode.Value;
                    }

                    return new GraphNodeQuery
                    {
                        Key = key,
                        Tags = tags,
                        Alias = alias,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }

    private static Option<GraphEdgeQuery> ParseEdge(Stack<LangNode> stack, List<IGraphQL> list)
    {
        string? nodeKey = null!;
        string? fromKey = null!;
        string? toKey = null!;
        string? edgeType = null!;
        string? tags = null!;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "svalue" }:
                    if (tags != null) return (StatusCode.BadRequest, "Tags already specified");
                    tags = langNode.Value;
                    break;

                case { SyntaxNode.Name: "lvalue" }:
                    string lvalue = langNode.Value.ToLower();

                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

                    switch (lvalue)
                    {
                        case "nodekey" when nodeKey == null: nodeKey = rvalue.Value; break;
                        case "nodekey" when nodeKey != null: return (StatusCode.BadRequest, "NodeKey already specified");

                        case "fromkey" when fromKey == null: fromKey = rvalue.Value; break;
                        case "fromkey" when fromKey != null: return (StatusCode.BadRequest, "FromKey already specified");

                        case "tokey" when toKey == null: toKey = rvalue.Value; break;
                        case "tokey" when toKey != null: return (StatusCode.BadRequest, "ToKey already specified");

                        case "edgetype" when edgeType == null: edgeType = rvalue.Value; break;
                        case "edgetype" when edgeType != null: return (StatusCode.BadRequest, "EdgeType already specified");

                        case "tags" when tags == null: tags = rvalue.Value; break;
                        case "tags" when tags != null: return (StatusCode.BadRequest, "Tags already specified");

                        default:
                            return (StatusCode.BadRequest, $"Only 'NodeKey', 'FromKey', 'ToKey', 'EdgeType', and/or 'Tags' is valid in lvalue");
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

                    return new GraphEdgeQuery
                    {
                        NodeKey = nodeKey,
                        FromKey = fromKey,
                        ToKey = toKey,
                        EdgeType = edgeType,
                        Tags = tags,
                        Alias = alias,
                    };

                default:
                    break;
            }
        }

        return (StatusCode.BadRequest, "No closure");
    }
}


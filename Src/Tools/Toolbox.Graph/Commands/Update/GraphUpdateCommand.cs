//using System.Collections.Immutable;
//using System.Diagnostics;
//using Toolbox.Extensions;
//using Toolbox.LangTools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public static class GraphUpdateCommand
//{
//    public static Option<IGraphQL> Parse(Stack<LangNode> stack)
//    {
//        var selectListOption = GraphSelectCommand.Parse(stack, "update");
//        if (selectListOption.IsError()) return selectListOption.ToOptionStatus<IGraphQL>();

//        GsSelect selectList = selectListOption.Return();
//        if (selectList.Search.Length == 0) return (StatusCode.BadRequest, "No search for delete");

//        if (!stack.TryPop(out var setCommand) || setCommand.SyntaxNode.Name != "update-set") return (StatusCode.BadRequest, "'set' command required");

//        switch (selectList.Search.Last())
//        {
//            case GraphNodeSearch:
//                var nodeParse = ParseNode(stack);
//                if (nodeParse.IsOk())
//                {
//                    return nodeParse.Return() with
//                    {
//                        Search = selectList.Search,
//                    };
//                }
//                break;

//            case GraphEdgeSearch:
//                var edgeParse = ParseEdge(stack);
//                if (edgeParse.IsOk())
//                {
//                    return edgeParse.Return() with
//                    {
//                        Search = selectList.Search,
//                    };
//                }
//                break;

//            case object v: throw new UnreachableException($"Unknown search type {v.GetType().FullName}");
//        }

//        return (StatusCode.BadRequest, "No language nodes");
//    }

//    private static Option<GsNodeUpdate> ParseNode(Stack<LangNode> stack)
//    {
//        var tags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
//        var dataMap = new Dictionary<string, GraphDataSource>(StringComparer.OrdinalIgnoreCase);

//        while (stack.TryPop(out var langNode))
//        {
//            switch (langNode)
//            {
//                case { SyntaxNode.Name: "svalue" }:
//                    if (!tags.TryAdd(langNode.Value, null)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
//                    break;

//                case { SyntaxNode.Name: "lvalue" }:
//                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
//                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

//                    if (!tags.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
//                    break;

//                case { SyntaxNode.Name: "dataName" }:
//                    var dataGroupOption = GraphDataMapParser.GetGraphDataLink(stack, langNode.Value);
//                    if (dataGroupOption.IsError()) return dataGroupOption.ToOptionStatus<GsNodeUpdate>();

//                    var data = dataGroupOption.Return();
//                    dataMap.Add(langNode.Value, data);
//                    break;

//                case { SyntaxNode.Name: "delimiter" }:
//                    break;

//                case { SyntaxNode.Name: "term" }:
//                    if (tags == null) return (StatusCode.BadRequest, "No key and tags must be specified");

//                    return new GsNodeUpdate
//                    {
//                        Tags = tags.ToTags(),
//                        DataMap = dataMap.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase),
//                    };

//                default: throw new UnreachableException($"Unknown langNode={langNode.GetType().FullName}");
//            }
//        }

//        return (StatusCode.BadRequest, "No closure");
//    }

//    private static Option<GsEdgeUpdate> ParseEdge(Stack<LangNode> stack)
//    {
//        string? edgeType = null!;
//        var tags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

//        while (stack.TryPop(out var langNode))
//        {
//            switch (langNode)
//            {
//                case { SyntaxNode.Name: "svalue" }:
//                    if (!tags.TryAdd(langNode.Value, null)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
//                    break;

//                case { SyntaxNode.Name: "lvalue" }:
//                    if (!stack.TryPop(out var equal) || equal.SyntaxNode.Name != "equal") return (StatusCode.BadRequest, "No equal");
//                    if (!stack.TryPop(out var rvalue) || rvalue.SyntaxNode.Name != "rvalue") return (StatusCode.BadRequest, "No rvalue");

//                    switch (langNode.Value.ToLower())
//                    {
//                        case "edgetype" when edgeType == null: edgeType = rvalue.Value; break;
//                        case "edgetype" when edgeType != null: return (StatusCode.BadRequest, "EdgeType already specified");

//                        default:
//                            if (!tags.TryAdd(langNode.Value, rvalue.Value)) return (StatusCode.BadRequest, $"Duplicate tag={langNode.Value}");
//                            break;
//                    }

//                    break;

//                case { SyntaxNode.Name: "delimiter" }:
//                    break;

//                case { SyntaxNode.Name: "term" }:
//                    if (edgeType == null && tags == null) return (StatusCode.BadRequest, "No edgeType, tags are required");

//                    return new GsEdgeUpdate
//                    {
//                        EdgeType = edgeType,
//                        Tags = tags.ToTags(),
//                    };

//                default: throw new UnreachableException($"Unknown langNode={langNode.GetType().FullName}");
//            }
//        }

//        return (StatusCode.BadRequest, "No closure");
//    }
//}
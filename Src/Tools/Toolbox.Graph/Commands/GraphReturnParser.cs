//using System.Collections.Immutable;
//using Toolbox.LangTools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GraphReturnParser
//{
//    public static Option<ImmutableHashSet<string>> ParseReturnNames(Stack<LangNode> stack)
//    {
//        var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

//        while (stack.TryPop(out var langNode))
//        {
//            switch (langNode)
//            {
//                case { SyntaxNode.Name: "svalue" }:
//                    if (!hashSet.Add(langNode.Value)) return (StatusCode.BadRequest, $"Duplicate dataName={langNode.Value}");
//                    break;

//                case { SyntaxNode.Name: "delimiter" }:
//                    break;

//                case { SyntaxNode.Name: "term" }:
//                    stack.Push(langNode);
//                    return hashSet.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

//                default:
//                    return (StatusCode.BadRequest, $"Unknown node, SyntaxNode.Name={langNode.SyntaxNode.Name}");
//            }
//        }

//        return (StatusCode.BadRequest, "No closure on data");
//    }

//}

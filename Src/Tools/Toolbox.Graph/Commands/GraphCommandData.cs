using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphCommandData
{
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

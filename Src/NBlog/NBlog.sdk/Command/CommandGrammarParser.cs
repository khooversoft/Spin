using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class CommandGrammarParser
{
    public static Option<IReadOnlyList<CommandNode>> Parse(string rawData)
    {
        LangResult langResult = CommandGrammar.Root.Parse(rawData);
        if (langResult.IsError()) return new Option<IReadOnlyList<CommandNode>>(langResult.StatusCode, langResult.Error);

        Stack<LangNode> stack = langResult.LangNodes.NotNull().Reverse().ToStack();

        string? fileId = null;
        string? localFilePath = null;
        List<string>? attributes = null;

        while (stack.TryPop(out var langNode))
        {
            switch (langNode)
            {
                case { SyntaxNode.Name: "attribute-group" } and { Value: "[" }:
                    attributes = GetAttributes(stack);
                    break;

                case { SyntaxNode.Name: "fileId" }:
                    fileId = langNode.Value;
                    break;

                case { SyntaxNode.Name: "equal" }:
                    break;

                case { SyntaxNode.Name: "localFilePath" }:
                    localFilePath = langNode.Value;

                    if (fileId.IsEmpty()) return (StatusCode.BadRequest, "No fileId");

                    var commandNode = new CommandNode
                    {
                        Attributes = attributes?.ToArray() ?? Array.Empty<string>(),
                        FileId = fileId,
                        LocalFilePath = localFilePath,
                    };

                    if (!commandNode.Validate(out Option v)) return v.ToOptionStatus<IReadOnlyList<CommandNode>>();
                    return (CommandNode[])[commandNode];

                default:
                    stack.Clear();
                    break;
            }
        }

        throw new UnreachableException("Failed to decode lang nodes");
    }

    private static List<string> GetAttributes(Stack<LangNode> stack)
    {
        var attributeList = new List<string>();

        while (stack.TryPop(out var attributeNode))
        {
            switch (attributeNode)
            {
                case { SyntaxNode.Name: "attributeName" }:
                    attributeList.Add(attributeNode.Value);
                    break;

                case { SyntaxNode.Name: "delimiter" }:
                    break;

                case { SyntaxNode.Name: "attribute-group" }:
                    return attributeList;
            }
        }

        return attributeList;
    }
}

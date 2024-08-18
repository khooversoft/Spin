using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class SyntaxParser
{
    private readonly MetaSyntaxRoot _syntaxRoot;
    private readonly IReadOnlyList<string> _parseTokens;
    private readonly IReadOnlyList<ProductionRule> _rootRules;

    public SyntaxParser(MetaSyntaxRoot syntaxRoot)
    {
        _syntaxRoot = syntaxRoot.NotNull();
        _parseTokens = _syntaxRoot.GetParseTokens().Assert(x => x.Count > 0, "No parse tokens found in meta schema");
        _rootRules = _syntaxRoot.GetRootRules().Assert(x => x.Count > 0, "No root rules");
    }

    public SyntaxResponse Parse(string rawData, ScopeContext context)
    {
        context.LogInformation("Parsing rawData={rawData}", rawData);

        var tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .Add(_parseTokens)
            .SetFilter(x => x.Value.IsNotEmpty())
            .Parse(rawData);

        context.LogInformation("Parsed data, tokens.Count={tokensCount}", tokens.Count);

        var pContext = new SyntaxParserContext(tokens, context);

        Option status2 = (StatusCode.BadRequest, "No tokens");
        while (pContext.TokensCursor.TryPeekValue(out var _))
        {
            status2 = ProcessRules(pContext, context);
            if (status2.IsError()) break;
        }

        status2.LogStatus(context, "Parsing rules exit");

        Option status = status2.IsError() switch
        {
            true => status2,
            false => pContext.TokensCursor.TryPeekValue(out var _) switch
            {
                true => (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens reached before completing")),
                false => StatusCode.OK,
            }
        };

        status.LogStatus(context, "Parse completed");

        var result = new SyntaxResponse
        {
            StatusCode = status.StatusCode,
            Error = status.Error,
            SyntaxTree = pContext.SyntaxTree.ConvertTo(),
        };

        return result;
    }

    private Option ProcessRules(SyntaxParserContext pContext, ScopeContext context)
    {
        context.LogInformation("ProcessingRules: pContext{pContext}", pContext.GetDebuggerDisplay(true));

        foreach (var rule in _rootRules)
        {
            var treeBuilder = new SyntaxTreeBuilder { MetaSyntax = rule };
            Option status = ProcessRule(pContext, rule, treeBuilder, context);
            if (status.IsOk() && treeBuilder.Children.Count > 0) pContext.SyntaxTree.Children.Add(treeBuilder.ConvertTo());

            if (status.IsNotFound()) continue;
            return status;
        }

        return (StatusCode.BadRequest, "No rules matched");
    }

    private Option ProcessRule(SyntaxParserContext pContext, IMetaSyntax parentMetaSyntax, SyntaxTreeBuilder tree, ScopeContext context)
    {
        using var scope = pContext.PushWithScope();
        context.LogInformation(
            "ProcessRule: pContext{pContext}, metaSyntax=[{metaSyntax}]",
            pContext.GetDebuggerDisplay(true), parentMetaSyntax.GetDebuggerDisplay()
            );

        var stack = new Stack<IMetaSyntax>(GetMetaSyntaxList(parentMetaSyntax).Reverse());
        ProductionRule? parentRule = parentMetaSyntax as ProductionRule;

        Option returnStatus = (StatusCode.BadRequest, "No rules to process");
        while (stack.TryPop(out var syntax))
        {
            switch (syntax)
            {
                case TerminalSymbol terminal:
                    returnStatus = ProcessTerminal(pContext, terminal, tree, context);
                    break;

                case VirtualTerminalSymbol virtualTerminal:
                    returnStatus = ProcessVirtualTerminal(pContext, virtualTerminal, tree, context);
                    break;

                case ProductionRule rule:
                    var treeBuilder = new SyntaxTreeBuilder { MetaSyntax = rule };
                    returnStatus = ProcessRule(pContext, syntax, treeBuilder, context);
                    if (returnStatus.IsOk() && treeBuilder.Children.Count > 0) tree.Children.Add(treeBuilder.ConvertTo());

                    if (returnStatus.IsNotFound() && rule.Type == ProductionRuleType.Optional)
                    {
                        returnStatus = StatusCode.OK;
                        continue;
                    }

                    if (returnStatus.IsOk() && rule.Type == ProductionRuleType.Repeat)
                    {
                        stack.Push(syntax);
                        continue;
                    }

                    if (returnStatus.IsNotFound() && rule.Type == ProductionRuleType.Repeat)
                    {
                        returnStatus = StatusCode.OK;
                        break;
                    }

                    break;

                case ProductionRuleReference referenceRule:
                    if (!_syntaxRoot.Nodes.TryGetValue(referenceRule.ReferenceSyntax, out var referenceSyntax))
                    {
                        context.LogError("ProcessRule: Reference syntax={referenceSyntax} not found", referenceRule.ReferenceSyntax);
                        return (StatusCode.NotFound, pContext.ErrorMessage($"Reference syntax={referenceRule.ReferenceSyntax} not found"));
                    }

                    stack.Push(referenceSyntax);
                    continue;

                default:
                    throw new UnreachableException();
            }

            if (parentRule?.Type == ProductionRuleType.Or && returnStatus.IsOk()) break;
            if (parentRule?.Type == ProductionRuleType.Or && returnStatus.IsNotFound()) continue;
            if (returnStatus.IsError()) break;
        }

        if (returnStatus.IsError())
        {
            returnStatus.LogStatus(context, "ProcessRule: Error");
            context.LogError("ProcessRule: Failed to match rule, pContext{pContext}", pContext.GetDebuggerDisplay(true));
            return StatusCode.NotFound;
        }

        scope.Cancel();
        context.LogInformation("ProcessRule: Success, pContext{pContext}", pContext.GetDebuggerDisplay(true));
        return returnStatus;

        static IReadOnlyList<IMetaSyntax> GetMetaSyntaxList(IMetaSyntax metaSyntax)
        {
            if (metaSyntax is TerminalSymbol terminalSymbol) return [terminalSymbol];
            if (metaSyntax is ProductionRuleReference productionRuleReference) return [productionRuleReference];
            if (metaSyntax is ProductionRule productionRule) return productionRule.Children;

            throw new ArgumentException($"Unknown metaSyntax type, metaSyntax.Type={metaSyntax.GetType().FullName}");
        };
    }

    private Option ProcessTerminal(SyntaxParserContext pContext, TerminalSymbol terminal, SyntaxTreeBuilder tree, ScopeContext context)
    {
        return ProcessTerminalValue(pContext, terminal, tree, isTokenMatch, context);

        bool isTokenMatch(string value) => terminal.Type switch
        {
            TerminalType.String => true,
            TerminalType.Token => value == terminal.Text,
            TerminalType.Regex => Regex.IsMatch(value, terminal.Text),

            _ => throw new UnreachableException(),
        };
    }

    private Option ProcessVirtualTerminal(SyntaxParserContext pContext, VirtualTerminalSymbol terminal, SyntaxTreeBuilder tree, ScopeContext context)
    {
        return ProcessTerminalValue(pContext, terminal, tree, x => x == terminal.Text, context);
    }

    private Option ProcessTerminalValue(SyntaxParserContext pContext, IMetaSyntax terminal, SyntaxTreeBuilder tree, Func<string, bool> isTokenMatch, ScopeContext context)
    {
        using var scope = pContext.PushWithScope();

        if (!pContext.TokensCursor.TryNextValue(out var current))
        {
            context.LogWarning("ProcessTerminal: No token found, pContext{pContext}", pContext.GetDebuggerDisplay(true));
            return (StatusCode.NotFound, $"ProcessTerminal: No token found, pContext={pContext.GetDebuggerDisplay(true)}");
        }

        switch (current)
        {
            case var v when v.TokenType == TokenType.Token && isTokenMatch(v.Value):
                tree.Children.Add(new SyntaxPair { Token = v, MetaSyntaxName = terminal.Name });
                context.LogInformation("Add Terminal: token=[{token}], terminal=[{terminal}]", v.GetDebuggerDisplay(), terminal.GetDebuggerDisplay());
                scope.Cancel();
                return StatusCode.OK;

            case var v when v.TokenType == TokenType.Block && isTokenMatch(v.Value):
                tree.Children.Add(new SyntaxPair { Token = v, MetaSyntaxName = terminal.Name });
                context.LogInformation("Add Terminal: block=[{block}], terminal=[{terminal}]", v.GetDebuggerDisplay(), terminal.GetDebuggerDisplay());
                scope.Cancel();
                return StatusCode.OK;

            default:
                context.LogWarning("ProcessTerminal: Not a terminal, pContext={pContext}", pContext.GetDebuggerDisplay(true));
                return StatusCode.NotFound;
        };
    }
}

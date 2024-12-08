using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
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
        _parseTokens = _syntaxRoot.Delimiters.Assert(x => x.Count > 0, "No parse tokens found in meta schema");
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
            Status = status,
            SyntaxTree = pContext.SyntaxTree.ConvertTo(),
        };

        return result;
    }

    private Option ProcessRules(SyntaxParserContext pContext, ScopeContext context)
    {
        context.LogInformation("ProcessingRules: pContext{pContext}", pContext.GetDebuggerDisplay());

        foreach (var rule in _rootRules)
        {
            context.LogTrace("[Rule] processing: pContext{pContext}", rule.GetDebuggerDisplay());

            var treeBuilder = new SyntaxTreeBuilder { MetaSyntax = rule };
            Option status = ProcessRule(pContext, rule, treeBuilder, context);
            if (status.IsOk() && treeBuilder.Children.Count > 0) pContext.SyntaxTree.Children.Add(treeBuilder.ConvertTo());

            if (status.IsNotFound())
            {
                context.LogTrace("[Rule] - no match - processing: pContext{pContext}", rule.GetDebuggerDisplay());
                continue;
            }

            return status;
        }

        return (StatusCode.BadRequest, "No rules matched");
    }

    private Option ProcessRule(SyntaxParserContext pContext, IMetaSyntax parentMetaSyntax, SyntaxTreeBuilder tree, ScopeContext context)
    {
        using var scope = pContext.PushWithScope();
        context.LogTrace(
            "ProcessRule: pContext{pContext}, metaSyntax=[{metaSyntax}]",
            pContext.GetDebuggerDisplay(), parentMetaSyntax.GetDebuggerDisplay()
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
            context.LogTrace("ProcessRule: Failed to match rule, pContext{pContext}", pContext.GetDebuggerDisplay());
            return StatusCode.NotFound;
        }

        scope.Cancel();
        context.LogTrace("ProcessRule: Success, pContext{pContext}", pContext.GetDebuggerDisplay());
        return returnStatus;

        static IReadOnlyList<IMetaSyntax> GetMetaSyntaxList(IMetaSyntax metaSyntax) => metaSyntax switch
        {
            TerminalSymbol terminalSymbol => [terminalSymbol],
            ProductionRuleReference productionRuleReference => [productionRuleReference],
            ProductionRule productionRule => productionRule.Children,

            _ => throw new ArgumentException($"Unknown metaSyntax type, metaSyntax.Type={metaSyntax.GetType().FullName}")
        };
    }

    private Option ProcessTerminal(SyntaxParserContext pContext, TerminalSymbol terminal, SyntaxTreeBuilder tree, ScopeContext context)
    {
        return ProcessTerminalValue(pContext, terminal, tree, isTokenMatch, context);

        bool isTokenMatch(string value) => terminal.Type switch
        {
            TerminalType.String => true,
            TerminalType.Token => value == terminal.Text,
            TerminalType.Regex => !isReserveWord(value) && Regex.IsMatch(value, terminal.Text),

            _ => throw new UnreachableException(),
        };

        bool isReserveWord(string value) => _syntaxRoot.ReserveWords.Contains(value);
    }

    private Option ProcessVirtualTerminal(SyntaxParserContext pContext, VirtualTerminalSymbol terminal, SyntaxTreeBuilder tree, ScopeContext context)
    {
        return ProcessTerminalValue(pContext, terminal, tree, x => x == terminal.Text, context);
    }

    private Option ProcessTerminalValue(SyntaxParserContext pContext, IMetaSyntax terminal, SyntaxTreeBuilder tree, Func<string, bool> isTokenMatch, ScopeContext context)
    {
        using var scope = pContext.PushWithScope();

        if (!pContext.TokensCursor.TryGetValue(out var current))
        {
            context.LogTrace("ProcessTerminal: No token found, pContext{pContext}", pContext.GetDebuggerDisplay());
            return (StatusCode.NotFound, $"ProcessTerminal: No token found, pContext={pContext.GetDebuggerDisplay()}");
        }

        switch (current)
        {
            case var v when isTokenMatch(v.Value):
                tree.Children.Add(new SyntaxPair { Token = v, MetaSyntaxName = terminal.Name });
                context.LogTrace("ProcessTerminal: Add Terminal: token=[{token}], terminal=[{terminal}]", v.GetDebuggerDisplay(), terminal.GetDebuggerDisplay());
                scope.Cancel();
                return StatusCode.OK;

            default:
                context.LogTrace("ProcessTerminal: Terminal does not match, currentToken={tokenValue}, terminal={terminal} pContext={pContext}",
                    current.Value, terminal.GetDebuggerDisplay(), pContext.GetDebuggerDisplay()
                    );

                return StatusCode.NotFound;
        };
    }
}

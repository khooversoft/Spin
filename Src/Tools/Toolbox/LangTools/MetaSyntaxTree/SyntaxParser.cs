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

        Option status2 = StatusCode.OK;
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
            Option status = ProcessRule(pContext, rule, pContext.SyntaxTree, context);

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

        bool success = false;
        while (!success && stack.TryPop(out var syntax))
        {
            switch (syntax)
            {
                case TerminalSymbol terminal:
                    var s1 = ProcessTerminal(pContext, terminal, tree, context);
                    if (s1.IsNotFound() && parentRule?.EvaluationType == EvaluationType.Or) continue;
                    if (s1.IsError()) return s1;
                    if (s1.IsOk() && parentRule?.EvaluationType == EvaluationType.Or) success = true;
                    break;

                case VirtualTerminalSymbol virtualTerminal:
                    var s2 = ProcessVirtualTerminal(pContext, virtualTerminal, tree, context);
                    if (s2.IsNotFound() && parentRule?.EvaluationType == EvaluationType.Or) continue;
                    if (s2.IsError()) return s2;
                    if (s2.IsOk() && parentRule?.EvaluationType == EvaluationType.Or) success = true;
                    break;

                case ProductionRule rule:
                    var ruleTree = new SyntaxTreeBuilder { MetaSyntax = rule };
                    Option s3 = ProcessRule(pContext, syntax, ruleTree, context);

                    if (s3.IsError() && rule.Type == ProductionRuleType.Repeat) continue;
                    if (s3.IsNotFound() && parentRule?.EvaluationType == EvaluationType.Or) continue;
                    if (s3.IsError() && rule.Type == ProductionRuleType.Optional) continue;
                    if (s3.IsError()) return s3;
                    if (s3.IsOk() && parentRule?.EvaluationType == EvaluationType.Or) success = true;
                    if (s3.IsOk()) tree.Children.Add(ruleTree.ConvertTo());

                    if (s3.IsOk() && rule.Type == ProductionRuleType.Repeat)
                    {
                        stack.Push(syntax);
                        continue;
                    }
                    break;

                case ProductionRuleReference referenceRule:
                    if (!_syntaxRoot.Nodes.TryGetValue(referenceRule.ReferenceSyntax, out var referenceSyntax))
                    {
                        context.LogError("ProcessRule: Reference syntax={referenceSyntax} not found", referenceRule.ReferenceSyntax);
                        return (StatusCode.NotFound, pContext.ErrorMessage($"Reference syntax={referenceRule.ReferenceSyntax} not found"));
                    }

                    stack.Push(referenceSyntax);
                    break;

                default:
                    throw new UnreachableException();
            }
        }

        if (parentRule?.EvaluationType == EvaluationType.Or && !success)
        {
            context.LogError("ProcessRule: No rules matched, pContext{pContext}", pContext.GetDebuggerDisplay(true));
            return StatusCode.NotFound;
        }

        scope.Cancel();
        context.LogInformation("ProcessRule: Success, pContext{pContext}", pContext.GetDebuggerDisplay(true));
        return StatusCode.OK;

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

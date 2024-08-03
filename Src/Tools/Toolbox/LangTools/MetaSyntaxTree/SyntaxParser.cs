using System.Collections.Immutable;
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
        _parseTokens = _syntaxRoot.GetParseTokens();
        _rootRules = _syntaxRoot.GetRootRules().Assert(x => x.Count > 0, "No root rules");
    }

    public Option Parse(string rawData, ScopeContext context)
    {
        _parseTokens.Count.Assert(x => x > 0, "No parse tokens found in meta schema");
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

        Option status = (status2.IsOk() && pContext.TokensCursor.TryPeekValue(out var _)) switch
        {
            false => StatusCode.OK,
            true => (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens reached before completing")),
        };

        return status.LogStatus(context, "Parse exist");
    }

    private Option ProcessRules(SyntaxParserContext pContext, ScopeContext context)
    {
        context.LogInformation("ProcessingRules: pContext{pContext}", pContext.GetDebuggerDisplay(true));

        foreach (var rule in _rootRules)
        {
            Option status = ProcessRule(pContext, rule, context);

            if (status.IsNotFound()) continue;
            if (!status.IsOk()) return status;
        }

        return StatusCode.OK;
    }
    private Option ProcessRule(SyntaxParserContext pContext, IMetaSyntax metaSyntax, ScopeContext context)
    {
        using var scope = pContext.PushWithScope();
        context.LogInformation("ProcessRule: pContext{pContext}, metaSyntax=[{metaSyntax}]", pContext.GetDebuggerDisplay(true), metaSyntax.GetDebuggerDisplay());

        switch (metaSyntax)
        {
            case TerminalSymbol terminal:
                var s1 = ProcessTerminal(pContext, terminal, context);
                if (s1.IsError()) return s1;
                break;

            case ProductionRule productionRule when productionRule.EvaluationType == EvaluationType.Sequence:
                var ruleCursor = productionRule.Children.ToCursor();
                var s2 = ProcessRuleChildren(pContext, ruleCursor, context);
                if (s2.IsError()) return s2;
                break;

            case ProductionRule productionOrRule when productionOrRule.EvaluationType == EvaluationType.Or:
                var ruleOrCursor = productionOrRule.Children.ToCursor();
                var s3 = ProcessRuleOrChildren(pContext, ruleOrCursor, context);
                if (s3.IsError()) return s3;
                break;

            case ProductionRule productionOptionalRule when productionOptionalRule.Type == ProductionRuleType.Optional:
                var ruleOptionalCursor = productionOptionalRule.Children.ToCursor();
                var s4 = ProcessRuleChildren(pContext, ruleOptionalCursor, context);
                if (s4.IsError())
                {
                    context.LogInformation(
                        "ProcessRule: Optional rule not found, ignoring, tokensCursor.Index={tokensCursorIndex}, metaSyntax=[{metaSyntax}]",
                        pContext.TokensCursor.Index, metaSyntax.GetDebuggerDisplay()
                        );
                }

                return StatusCode.NotFound;

            default:
                throw new UnreachableException();
        }

        scope.Cancel();
        context.LogInformation("ProcessRule: Success, pContext{pContext}", pContext.GetDebuggerDisplay(true));
        return StatusCode.OK;
    }

    private Option ProcessRuleChildren(SyntaxParserContext pContext, Cursor<IMetaSyntax> ruleCursor, ScopeContext context)
    {
        context.LogInformation("ProcessRuleChildren: enter - pContext{pContext}", pContext.GetDebuggerDisplay(true));

        while (pContext.TokensCursor.TryNextValue(out var token) && ruleCursor.TryNextValue(out var rule))
        {
            context.LogInformation("ProcessRuleChildren: token=[{token}], rule=[{rule}]", token.GetDebuggerDisplay(), rule.GetDebuggerDisplay());
            context.LogInformation("ProcessRuleChildren: tryNextValue - pContext{pContext}", pContext.GetDebuggerDisplay(true));

            switch (rule)
            {
                case ProductionRuleReference reference:
                    if (!_syntaxRoot.Nodes.TryGetValue(reference.ReferenceSyntax, out var syntax))
                    {
                        context.LogError("ProcessRuleChildren: Reference syntax={referenceSyntax} not found", reference.ReferenceSyntax);
                        return (StatusCode.NotFound, pContext.ErrorMessage($"Reference syntax={reference.ReferenceSyntax} not found"));
                    }

                    var s0 = ProcessRule(pContext, syntax, context);
                    if (s0.IsError()) return s0;
                    break;

                case TerminalSymbol terminal:
                    var s1 = ProcessTerminal(pContext, terminal, context);
                    if (s1.IsError()) return s1;
                    break;

                case ProductionRule productionRule:
                    var s2 = ProcessRule(pContext, rule, context);
                    if (s2.IsNotFound()) continue;
                    if (s2.IsError()) return s2;
                    break;

                default:
                    throw new UnreachableException();
            }
        }

        return StatusCode.OK;
    }

    private Option ProcessRuleOrChildren(SyntaxParserContext pContext, Cursor<IMetaSyntax> ruleCursor, ScopeContext context)
    {
        context.LogInformation("ProcessRuleOrChildren: enter - pContext{pContext}", pContext.GetDebuggerDisplay(true));

        while (ruleCursor.TryNextValue(out var rule))
        {
            IToken token = pContext.TokensCursor.Current;

            switch (rule)
            {
                case ProductionRuleReference reference:
                    if (!_syntaxRoot.Nodes.TryGetValue(reference.ReferenceSyntax, out var syntax))
                    {
                        context.LogError("ProcessRuleOrChildren: Reference syntax={referenceSyntax} not found", reference.ReferenceSyntax);
                        return (StatusCode.NotFound, pContext.ErrorMessage($"Reference syntax={reference.ReferenceSyntax} not found"));
                    }

                    var s0 = ProcessRule(pContext, syntax, context);
                    if (s0.IsOk()) return s0;
                    break;

                case TerminalSymbol terminal:
                    var s1 = ProcessTerminal(pContext, terminal, context);
                    if (s1.IsOk()) return s1;
                    break;

                case ProductionRule productionRule:
                    var s2 = ProcessRule(pContext, rule, context);
                    if (s2.IsOk()) return s2;
                    break;

                default:
                    throw new UnreachableException();
            }
        }

        return StatusCode.OK;
    }

    private Option ProcessTerminal(SyntaxParserContext pContext, TerminalSymbol terminal, ScopeContext context)
    {
        switch (pContext.TokensCursor.Current)
        {
            case var v when v.TokenType == TokenType.Token && isTokenMatch(v.Value):
                pContext.Pairs.Add(new SyntaxPair { Token = v, MetaSyntax = terminal });
                context.LogInformation("Add Terminal: token=[{token}], terminal=[{terminal}]", v.GetDebuggerDisplay(), terminal.GetDebuggerDisplay());
                return StatusCode.OK;

            default:
                context.LogError("ProcessTerminal: Not a terminal, pContext{pContext}", pContext.GetDebuggerDisplay(true));
                return StatusCode.NotFound;
        };

        bool isTokenMatch(string value) => terminal.Type switch
        {
            TerminalType.String => true,
            TerminalType.Token => value == terminal.Text,
            TerminalType.Regex => Regex.IsMatch(value, terminal.Text),

            _ => throw new UnreachableException(),
        };
    }
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct SyntaxPair
{
    public IToken Token { get; init; }
    public IMetaSyntax MetaSyntax { get; init; }

    public string GetDebuggerDisplay() => $"Token={Token.Value}, MetaSyntax.Name={MetaSyntax.Name}";
}

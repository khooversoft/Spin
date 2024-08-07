using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public static class MetaParser
{
    private const string _tagPattern = @"^[a-zA-Z][-\w]*\w$";
    private static readonly Regex _tagRegex = new Regex(_tagPattern);

    public static MetaSyntaxRoot ParseRules(string rules)
    {
        var tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .Add(MetaSyntaxTool.ParseTokens)
            .SetFilter(x => x.Value.IsNotEmpty())
            .Parse(rules);

        var pContext = new MetaParserContext(tokens);

        while (pContext.TokensCursor.TryPeekValue(out var _))
        {
            var s1 = ParseTerminal(pContext);
            if (s1.IsOk()) continue;

            var s2 = ParseProductionRule(pContext);
            if (s2.IsError()) return pContext.ConvertTo(s2);
        }

        Option status = pContext.TokensCursor.TryPeekValue(out var _) switch
        {
            false => StatusCode.OK,
            true => (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens reached before completing")),
        };

        return pContext.ConvertTo(status);
    }

    private static Option CreateError(string message, IToken token) => (StatusCode.BadRequest, message);

    private static Option ParseTerminal(MetaParserContext pContext)
    {
        using var scope = pContext.PushWithScope();

        if (!pContext.TokensCursor.TryNextValue(out IToken? nameToken)) return (StatusCode.BadRequest, pContext.ErrorMessage("Expected name token"));
        if (!pContext.TokensCursor.TryNextValue(out IToken? equalToken) || equalToken.Value != "=") return (StatusCode.BadRequest, pContext.ErrorMessage("Expected '='"));

        TerminalType terminalType = TerminalType.Token;

        bool isTerm = false;
        IToken? valueToken = null;
        var tags = new Sequence<string>();

        while (pContext.TokensCursor.TryNextValue(out IToken? token))
        {
            switch (token)
            {
                case var v when v.TokenType == TokenType.Token && v.Value == "string":
                    if (terminalType != TerminalType.Token) return (StatusCode.BadRequest, pContext.ErrorMessage("Modifier already specified, like 'regex'"));
                    terminalType = TerminalType.String;
                    continue;

                case var v when v.TokenType == TokenType.Token && v.Value == "regex":
                    if (terminalType != TerminalType.Token) return (StatusCode.BadRequest, pContext.ErrorMessage("Modifier already specified, like 'string'"));
                    terminalType = TerminalType.Regex;
                    continue;

                case var v when v.TokenType == TokenType.Token && v.Value == ";":
                    isTerm = true;
                    break;

                case var v when v.TokenType == TokenType.Token && v.Value.StartsWith("#"):
                    if( !IsTag(v.Value[1..])) return (StatusCode.BadRequest, pContext.ErrorMessage("Invalid tag"));
                    tags += v.Value[1..];
                    continue;

                default:
                    if (valueToken != null) return (StatusCode.BadRequest, pContext.ErrorMessage("Value token already specified"));
                    if (token.TokenType != TokenType.Block) return (StatusCode.BadRequest, pContext.ErrorMessage("Token is not a string literial"));
                    if (token.Value.IsEmpty()) return (StatusCode.BadRequest, pContext.ErrorMessage("Token is empty"));
                    valueToken = token;
                    continue;
            }

            break;
        }

        if (!isTerm) return (StatusCode.BadRequest, pContext.ErrorMessage("No term ';' token"));
        if (terminalType != TerminalType.String && valueToken == null) return (StatusCode.BadRequest, pContext.ErrorMessage("Expected value token"));

        scope.Cancel();

        var syntax = new TerminalSymbol
        {
            Name = nameToken.Value.NotEmpty(),
            Text = terminalType == TerminalType.String ? "string" : valueToken.NotNull().Value.NotEmpty(),
            Type = terminalType,
            Index = pContext.TokensCursor.Current.Index,
            Tags = tags.ToImmutableArray(),
        };

        pContext.Add(syntax);
        return StatusCode.OK;
    }

    public static Option ParseProductionRule(MetaParserContext pContext)
    {
        if (!pContext.TokensCursor.TryNextValue(out IToken? nameToken)) return (StatusCode.BadRequest, pContext.ErrorMessage("Expected name token"));
        if (!pContext.TokensCursor.TryNextValue(out IToken? equalToken) || equalToken.Value != "=") return (StatusCode.BadRequest, pContext.ErrorMessage("Expected '='"));

        var rule = new ProductionRuleBuilder { Name = nameToken.Value, Index = pContext.TokensCursor.Current.Index };

        var result = ParseProductionRule(pContext, rule);
        if (result.IsError()) return result;

        pContext.Add(rule.ConvertTo());
        return result;
    }

    private static Option ParseProductionRule(MetaParserContext pContext, ProductionRuleBuilder rule, string? endToken = null)
    {
        int tokensProcessed = 0;
        bool requireDelimiter = false;
        EvaluationType evalType = EvaluationType.None;

        while (pContext.TokensCursor.TryNextValue(out IToken? token))
        {
            tokensProcessed++;

            if (token.Value == ";")
            {
                if (endToken != null) return (StatusCode.BadRequest, pContext.ErrorMessage("Unexpected ';'"));
                if (tokensProcessed == 1) return (StatusCode.BadRequest, pContext.ErrorMessage("Expected token after '='"));
                return StatusCode.OK;
            }

            if (endToken != null && token.Value == endToken)
            {
                rule.EvaluationType = evalType;
                return StatusCode.OK;
            }

            switch (requireDelimiter)
            {
                case false when token.Value == "," || token.Value == "|": return (StatusCode.BadRequest, pContext.ErrorMessage("Not expecting ','"));

                case true when evalType == EvaluationType.None && token.Value == ",":
                    evalType = EvaluationType.Sequence;
                    requireDelimiter = false;
                    continue;

                case true when evalType == EvaluationType.None && token.Value == "|":
                    evalType = EvaluationType.Or;
                    requireDelimiter = false;
                    continue;

                case true when evalType == EvaluationType.Sequence && token.Value != ",": return (StatusCode.BadRequest, pContext.ErrorMessage("Not Expected"));
                case true when evalType == EvaluationType.Or && token.Value != "|": return (StatusCode.BadRequest, pContext.ErrorMessage("Not Expected"));

                case true: requireDelimiter = false; continue;
            }
            requireDelimiter = true;

            var t2 = tryProcessGroup(token);
            if (t2.IsError()) return t2;
            if (t2.IsOk()) continue;

            if (token is BlockToken tokenValue)
            {
                rule.Children.Add(new VirtualTerminalSymbol { Name = $"{CreateName(rule.Name)}-{tokensProcessed}", Text = tokenValue.Value });
                continue;
            }

            if (pContext.Nodes.TryGetValue(token.Value, out var syntax))
            {
                rule.Children.Add(new ProductionRuleReference { Name = $"{CreateName(rule.Name)}-{tokensProcessed}-{token.Value}", ReferenceSyntax = syntax.Name });
                continue;
            }

            return (StatusCode.BadRequest, pContext.ErrorMessage("Unknown token"));
        }

        return (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens without completing, missing end group or ';'"));

        Option tryProcessGroup(IToken token)
        {
            if (!MetaSyntaxTool.TryGetGroupToken(token.Value, out var groupToken)) return StatusCode.NoContent;

            var newRule = new ProductionRuleBuilder
            {
                Name = $"{CreateName(rule.Name)}-{tokensProcessed}-{groupToken.Label}",
                Type = groupToken.Type,
                Index = pContext.TokensCursor.Current.Index,
            };

            var ruleResult = ParseProductionRule(pContext, newRule, groupToken.CloseSymbol);
            if (ruleResult.StatusCode.IsOk()) rule.Children.Add(newRule.ConvertTo());

            return ruleResult;
        }
    }

    private static string CreateName(string name) => name.Length > 0 && name[0] == '_' ? name : $"_{name}";
    private static bool IsTag(string input) => _tagRegex.IsMatch(input);
}

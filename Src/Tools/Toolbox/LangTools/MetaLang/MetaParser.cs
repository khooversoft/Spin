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
            var s0 = ParseDelimitersCommands(pContext);
            if (s0.IsOk()) continue;

            var s1 = ParseReserveWordsCommands(pContext);
            if (s1.IsOk()) continue;

            var s2 = ParseTerminal(pContext);
            if (s2.IsOk()) continue;

            var s3 = ParseProductionRule(pContext);
            if (s3.IsError()) return pContext.ConvertTo(s3);
        }

        Option status = pContext.TokensCursor.TryPeekValue(out var _) switch
        {
            false => StatusCode.OK,
            true => (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens reached before completing")),
        };

        return pContext.ConvertTo(status);
    }

    private static Option CreateError(string message, IToken token) => (StatusCode.BadRequest, message);

    private static Option ParseDelimitersCommands(MetaParserContext pContext)
    {
        using var scope = pContext.PushWithScope();

        if (!pContext.TokensCursor.TryNextValue(out IToken? nameToken)) return (StatusCode.BadRequest, pContext.ErrorMessage("Expected name token"));
        if (nameToken.Value != "delimiters") return StatusCode.NotFound;
        if (!pContext.TokensCursor.TryNextValue(out IToken? equalToken) || equalToken.Value != "=") return (StatusCode.BadRequest, pContext.ErrorMessage("Expected '='"));

        while (pContext.TokensCursor.TryNextValue(out IToken? token))
        {
            if (token.Value == ";" && token.TokenType == TokenType.Token) break;
            pContext.Delimiters.Add(token.Value);
        }

        scope.Cancel();
        return StatusCode.OK;
    }

    private static Option ParseReserveWordsCommands(MetaParserContext pContext)
    {
        using var scope = pContext.PushWithScope();

        if (!pContext.TokensCursor.TryNextValue(out IToken? nameToken)) return (StatusCode.BadRequest, pContext.ErrorMessage("Expected name token"));
        if (nameToken.Value != "reserve-words") return StatusCode.NotFound;
        if (!pContext.TokensCursor.TryNextValue(out IToken? equalToken) || equalToken.Value != "=") return (StatusCode.BadRequest, pContext.ErrorMessage("Expected '='"));

        while (pContext.TokensCursor.TryNextValue(out IToken? token))
        {
            if (token.Value == ";" && token.TokenType == TokenType.Token) break;
            pContext.ReserveWords.Add(token.Value);
        }

        scope.Cancel();
        return StatusCode.OK;
    }

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
                    if (!IsTag(v.Value[1..])) return (StatusCode.BadRequest, pContext.ErrorMessage("Invalid tag"));
                    tags += v.Value[1..];
                    continue;

                default:
                    if (valueToken != null) return (StatusCode.BadRequest, pContext.ErrorMessage("Value token already specified"));

                    switch (token.TokenType)
                    {
                        case TokenType.Token when token.Value.IsNotEmpty():
                            if (!pContext.Nodes.TryGetValue(token.Value, out var referenceTerminal)) goto default;
                            if (referenceTerminal is TerminalSymbol terminalSymbol)
                            {
                                valueToken = new TokenValue(terminalSymbol.Text);
                                terminalType = terminalSymbol.Type;
                                continue;
                            }

                            return (StatusCode.BadRequest, pContext.ErrorMessage($"Cannot find terminal reference '{token.Value}'"));

                        case TokenType.Block:
                            valueToken = token;
                            continue;

                        default:
                            return (StatusCode.BadRequest, pContext.ErrorMessage("Token is not a string literial or a terminal reference"));
                    };
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
                return StatusCode.OK;
            }

            switch (requireDelimiter)
            {
                case false when token.Value == "," || token.Value == "|": return (StatusCode.BadRequest, pContext.ErrorMessage("Not expecting ','"));
                case false: break;

                case true when rule.Type == ProductionRuleType.Sequence && token.Value == ",":
                case true when rule.Type == ProductionRuleType.Optional && token.Value == ",":
                case true when rule.Type == ProductionRuleType.Repeat && token.Value == ",":
                    requireDelimiter = false;
                    continue;

                case true when rule.Type == ProductionRuleType.Or && token.Value == "|":
                    requireDelimiter = false;
                    continue;

                default:
                    return (StatusCode.BadRequest, pContext.ErrorMessage("Not Expected"));
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
            if (ruleResult.StatusCode.IsOk())
            {
                ProductionRule newRuleConverted = newRule.ConvertTo();
                rule.Children.Add(newRuleConverted);

                pContext.Nodes.TryAdd(newRuleConverted.Name, newRuleConverted)
                    .Assert(x => x == true, $"Syntax node '{newRuleConverted.Name}' already exists");
            }

            return ruleResult;
        }
    }

    private static string CreateName(string name) => name.Length > 0 && name[0] == '_' ? name : $"_{name}";
    private static bool IsTag(string input) => _tagRegex.IsMatch(input);
}

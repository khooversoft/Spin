using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public static class MetaParser
{
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
            true => (StatusCode.BadRequest, "End of tokens reached before completing"),
        };

        return pContext.ConvertTo(status);
    }

    private static Option CreateError(string message, IToken token) => (StatusCode.BadRequest, message);

    private static Option ParseTerminal(MetaParserContext pContext)
    {
        using var scope = pContext.PushWithScope();
        bool isRegex = false;

        if (!pContext.TokensCursor.TryNextValue(out IToken? nameToken)) return (StatusCode.BadRequest, "Expected name token");
        if (!pContext.TokensCursor.TryNextValue(out IToken? equalToken) || equalToken.Value != "=") return (StatusCode.BadRequest, "Expected '='");
        if (!pContext.TokensCursor.TryNextValue(out IToken? valueToken)) return (StatusCode.BadRequest, "Expected value token");

        if (valueToken.Value.EqualsIgnoreCase("regex:"))
        {
            if (!pContext.TokensCursor.TryNextValue(out IToken? regexToken)) return (StatusCode.BadRequest, "Expected regex token");
            isRegex = true;
            valueToken = regexToken;
        }

        if (valueToken.TokenType != TokenType.Block) return (StatusCode.BadRequest, "Token is not a string literial");
        if (!pContext.TokensCursor.TryNextValue(out IToken? termSymbol) || termSymbol.Value != ";") return (StatusCode.BadRequest, "Expected value token");

        scope.Cancel();

        var syntax = new TerminalSymbol
        {
            Name = nameToken.Value.NotEmpty(),
            Text = valueToken.Value.NotEmpty(),
            Regex = isRegex,
        };

        pContext.Add(syntax);
        return StatusCode.OK;
    }

    public static Option ParseProductionRule(MetaParserContext pContext)
    {
        if (!pContext.TokensCursor.TryNextValue(out IToken? nameToken)) return (StatusCode.BadRequest, "Expected name token");
        if (!pContext.TokensCursor.TryNextValue(out IToken? equalToken) || equalToken.Value != "=") return (StatusCode.BadRequest, "Expected '='");

        var rule = new ProductionRule { Name = nameToken.Value };
        pContext.Add(rule);

        var result = ParseProductionRule(pContext, rule);
        return result;
    }

    private static Option ParseProductionRule(MetaParserContext pContext, ProductionRule rule, string? endToken = null)
    {
        int tokensProcessed = 0;
        bool requireComma = false;

        while (pContext.TokensCursor.TryNextValue(out IToken? token))
        {
            tokensProcessed++;

            if (token.Value == ";")
            {
                if (endToken != null) return (StatusCode.BadRequest, "Unexpected ';'");
                if (tokensProcessed == 1) return (StatusCode.BadRequest, "Expected token after '='");
                return StatusCode.OK;
            }

            if (endToken != null && token.Value == endToken) return StatusCode.OK;

            switch (requireComma)
            {
                case false when token.Value == ",": return (StatusCode.BadRequest, "Not expecting ','");
                case true when token.Value != ",": return (StatusCode.BadRequest, "Expected ','");
                case true: requireComma = false; continue;
            }
            requireComma = true;

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
                rule.Children.Add(new ProductionRuleReference { Name = $"{CreateName(rule.Name)}-{tokensProcessed}", ReferenceSyntax = syntax });
                continue;
            }

            return (StatusCode.BadRequest, $"Unknown token={token.Value}");
        }

        return (StatusCode.BadRequest, "End of tokens without completing, missing end group or ';'");

        Option tryProcessGroup(IToken token)
        {
            if (!MetaSyntaxTool.TryGetGroupToken(token.Value, out var groupToken)) return StatusCode.NoContent;

            var newRule = new ProductionRule
            {
                Name = $"{CreateName(rule.Name)}-{tokensProcessed}",
                Type = groupToken.Type,
            };

            rule.Children.Add(newRule);

            var ruleResult = ParseProductionRule(pContext, newRule, groupToken.CloseSymbol);
            if (ruleResult.IsError()) return ruleResult;

            return StatusCode.OK;
        }
    }

    private static string CreateName(string name) => name.Length > 0 && name[0] == '_' ? name : $"_{name}";
}

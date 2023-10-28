using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data.Graph;

public enum QueryOpr
{
    Equal,
    Has,
    Match,
    And,
    Semicolon,
}

public readonly record struct QueryCmd
{
    public QueryOpr Opr { get; init; }
    public string? Symbol { get; init; }
    public string? Value { get; init; }
}


public class GraphCmdParser
{
    private enum TokenType
    {
        None,
        Symbol,
        Value,
        Opr,
    }

    private readonly record struct TokenResult
    {
        public TokenType Type { get; init; }
        public string? Value { get; init; }
        public static TokenResult None { get; } = new TokenResult { Type = TokenType.None };
    }

    private readonly record struct Test
    {
        public Func<IToken, TokenResult>[] Pattern { get; init; }
        public Func<TokenResult[], bool> PostTest { get; init; }
        public Func<TokenResult[], QueryCmd> Build { get; init; }
    }


    // toKey = 'value1' && fromKey = 'value2' && tags has 't2=v2' && tokey match 'schema:*'
    public static Option<IReadOnlyList<QueryCmd>> Parse(string queryCmd, params string[] parseTokens)
    {
        if (queryCmd.IsEmpty()) return StatusCode.BadRequest;

        parseTokens = parseTokens switch
        {
            { Length: 0 } => new string[] { "=", "has", "match", "&&", ";" },
            var v => v,
        };

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .Add(parseTokens)
            .Parse(queryCmd)
            .Where(x => x.Value.IsNotEmpty())
            .ToArray();

        var tests = new[]
        {
            EqualValue,
            HasValue,
            MatchValue,
            IsAnd,
            IsSemicolon,
        };

        var index = tokens.ToCursor();
        index.Index = 0;
        var list = new Sequence<QueryCmd>();

        while (!index.IsCursorAtEnd)
        {
            Option<QueryCmd> result = tests
                .Select(x => isMatch(index, x))
                .SkipWhile(x => x.IsError())
                .FirstOrDefault(StatusCode.NotFound);

            if (result.IsError()) return StatusCode.BadRequest;

            list += result.Return();
        }

        return list;
    }

    private static Option<QueryCmd> isMatch(Cursor<IToken> index, Test test)
    {
        var tokens = index.FromCursor(test.Pattern.Length);
        if (tokens.Count != test.Pattern.Length) return StatusCode.NotFound;

        TokenResult[] result = test.Pattern.Zip(tokens).Select(x => x.First(x.Second)).ToArray();
        if (result.Any(x => x == TokenResult.None)) return StatusCode.NotFound;

        if (!test.PostTest(result)) return StatusCode.NotFound;

        index.Index = index.Index + tokens.Count;
        return test.Build(result);
    }

    private static Test IsAnd = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => x.Value == "&&" ? new TokenResult { Type = TokenType.Opr } : TokenResult.None,
        },
        PostTest = x => x.Length == 1,
        Build = (tokens) => new QueryCmd
        {
            Opr = QueryOpr.And,
        },
    };

    private static Test IsSemicolon = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => x.Value == ";" ? new TokenResult { Type = TokenType.Opr } : TokenResult.None,
        },
        PostTest = x => x.Length == 1,
        Build = (tokens) => new QueryCmd
        {
            Opr = QueryOpr.Semicolon,
        },
    };

    private static Test EqualValue = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Symbol, Value = x.Value } : TokenResult.None,
            x => x.Value == "=" ? new TokenResult { Type = TokenType.Opr } : TokenResult.None,
            x => x.Value.IsNotEmpty() ? new TokenResult { Type = TokenType.Value, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Length == 3,
        Build = (tokens) => new QueryCmd
        {
            Symbol = tokens[0].Value.NotEmpty(),
            Opr = QueryOpr.Equal,
            Value = tokens[2].Value.NotEmpty(),
        },
    };

    private static Test HasValue = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Symbol, Value = x.Value } : TokenResult.None,
            x => x.Value.Equals("has", StringComparison.OrdinalIgnoreCase)  ? new TokenResult { Type = TokenType.Opr } : TokenResult.None,
            x => x.Value.IsNotEmpty() ? new TokenResult { Type = TokenType.Value, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Length == 3,
        Build = (tokens) => new QueryCmd
        {
            Symbol = tokens[0].Value.NotEmpty(),
            Opr = QueryOpr.Has,
            Value = tokens[2].Value.NotEmpty(),
        },
    };

    private static Test MatchValue = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Symbol, Value = x.Value } : TokenResult.None,
            x => x.Value.Equals("match", StringComparison.OrdinalIgnoreCase)  ? new TokenResult { Type = TokenType.Opr } : TokenResult.None,
            x => x.Value.IsNotEmpty() ? new TokenResult { Type = TokenType.Value, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Length == 3,
        Build = (tokens) => new QueryCmd
        {
            Symbol = tokens[0].Value.NotEmpty(),
            Opr = QueryOpr.Match,
            Value = tokens[2].Value.NotEmpty(),
        },
    };


}

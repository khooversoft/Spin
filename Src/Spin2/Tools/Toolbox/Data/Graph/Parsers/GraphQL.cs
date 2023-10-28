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

namespace Toolbox.Data;

public interface IGraphQL
{
}

public readonly record struct QlInstruction<T> : IGraphQL where T : IGraphQL
{
    public T Value { get; init; }
    public string? Name { get; init; }
}

/// <summary>
/// q = ( node | edge )[]
/// 
/// node = '(' {nodeQuery} ')'
/// edge = '[' {edgeQuery} ']'
/// 
/// nodeQuery = "key" = {nodeKey} |
///             "tags" = {has tag} |
///             {has tag}
///             
/// edgeQuery = "nodeKey" = {nodeKey} |
///             "fromKey" = {fromKey} |
///             "toKey" = {toKey} |
///             "type" = {edgeType} |
///             "tags" = {has tag} |
///             {has tag}
///             
/// Examples:
///     (key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
///     (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
///     (key= key1; t1) n1 -> [schedulework:*] -> (schedule) n2
///     [fromKey = key1; edgeType=abc*] -> (schedule) n1
///     (t1) -> [tags=schedulework:active] -> (tags="state=active") n1
///             
/// </summary>
public class GraphQL
{
    private enum TokenType
    {
        None,
        Symbol,
        Syntax,
        Open,
    }

    private readonly record struct TokenResult
    {
        public TokenType Type { get; init; }
        public string? Value { get; init; }
        public List<TokenResult>? Results { get; init; }
    }


    public static Option<IReadOnlyList<IGraphQL>> Parse(string query)
    {
        if (query.IsEmpty()) return StatusCode.BadRequest;

        IReadOnlyList<TokenValue> tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .Add("=", ";", "(", ")", "[", "]", "->")
            .Parse(query)
            .OfType<TokenValue>()
            .Where(x => x.Value.IsNotEmpty())
            .ToArray();

        var tokenResults = CreateTree(tokens);
        //var instructions = CreateInstructions(tokenResults);

        return null!;
    }

    //private static Option<IReadOnlyList<IGraphQL>> CreateInstructions(IReadOnlyList<TokenResult> tokenResults)
    //{
    //    var cursor = tokenResults.ToCursor();
    //    bool first = true;

    //    while (cursor.TryNextValue(out var result))
    //    {
    //        if (first)
    //        {
    //            first = false;
    //            if (result.Type != TokenType.Open) return (StatusCode.BadRequest, "Node or edge is not the first instruction");
    //        }

    //        switch (result)
    //        {
    //            case { Type: TokenType.Open, Value: "(" }:
    //                Option<GraphNode<string>> nodeResult = CreateNode(cursor);
    //                break;
    //        }
    //    }
    //}

    /// <summary>
    ///  (key=key1;tags=t1)
    ///  (key=key1;t1)
    ///  (t1)
    ///  [;]
    /// </summary>
    /// <param name="cursor"></param>
    /// <returns></returns>
    //private static Option<GraphNodeQuery<string>> CreateNode(Cursor<TokenResult> cursor)
    //{
    //    int save = cursor.Index;
    //    using var scope = new FinalizeScope<Cursor<TokenResult>>(cursor, x => x.Index = save);

    //    var list = new Sequence<GraphNodeQuery<string>>();
    //    var instrs = new List<string>();

    //    string? key = null;
    //    string? tags = null;

    //    while (cursor.TryNextValue(out var result))
    //    {
    //        switch (result)
    //        {
    //            case { Type: TokenType.Syntax, Value: ";" }:
    //            case { Type: TokenType.Syntax, Value: ")" }:
    //                switch (instrs)
    //                {
    //                    case { Count: 1 }:
    //                        if (tags != null) return (StatusCode.BadRequest, "syntax error: {tags} already set");
    //                        tags = instrs[0];
    //                        break;

    //                    case { Count: 3 }:
    //                        if (key != null) return (StatusCode.BadRequest, "syntax error: {key} already set");
    //                        if (!instrs[0].EqualsIgnoreCase("key")) return (StatusCode.BadRequest, $"Unknown symbol={instrs[0]}, requires 'key'");
    //                        if (instrs[1] != "=") return (StatusCode.BadRequest, $"Unknown operator={instrs[1]}");

    //                        key = instrs[2];
    //                        break;
    //                }
    //                continue;

    //            case { Type: TokenType.Syntax, Value: "=" }:
    //                instrs.Add("=");
    //                continue;

    //            case { Type: TokenType.Symbol }:
    //                instrs.Add(result.Value.NotNull());
    //                continue;

    //            default: return (StatusCode.BadRequest, $"Invalid syntax {result.Value}");
    //        }
    //    }

    //    return list;
    //}

    private static IReadOnlyList<TokenResult> CreateTree(IReadOnlyList<TokenValue> tokenValues)
    {
        var stack = tokenValues.Reverse().ToStack();
        var list = new List<TokenResult>();
        TokenResult? current = null;

        while (stack.TryPop(out var tokenValue))
        {
            switch (tokenValue)
            {
                case { Value: "(", IsSyntaxToken: true }:
                case { Value: "[", IsSyntaxToken: true }:
                    current = new TokenResult { Type = TokenType.Open, Value = tokenValue.Value, Results = new List<TokenResult>() };
                    list.Add((TokenResult)current);
                    break;

                case { Value: ")", IsSyntaxToken: true }:
                case { Value: "]", IsSyntaxToken: true }:
                    current = null;
                    break;

                default:
                    switch (current)
                    {
                        case null:
                            list.Add(new TokenResult
                            {
                                Type = tokenValue.IsSyntaxToken ? TokenType.Syntax : TokenType.Symbol,
                                Value = tokenValue.Value
                            });
                            break;

                        case TokenResult v:
                            v.Results.NotNull().Add(new TokenResult
                            {
                                Type = tokenValue.IsSyntaxToken ? TokenType.Syntax : TokenType.Symbol,
                                Value = tokenValue.Value
                            });
                            break;
                    }
                    break;
            }
        }

        return list;
    }
}

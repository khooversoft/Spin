using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Tokenizer.Tree;

public class LangParserContext
{
    private ConcurrentDictionary<Guid, Cursor<ILangSyntax>> _tracking = new ConcurrentDictionary<Guid, Cursor<ILangSyntax>>();

    public Cursor<ILangSyntax> GetCursor()
    {
        var result = _tracking.GetOrAdd(SyntaxCurrent.Id, _ => SyntaxCurrent.CreateCursor());
        return result;
    }

    public void Reset() => _tracking.Clear();

    public required ILangRoot SyntaxCurrent { get; init; }
    public required ILangTree WriteNode { get; init; }
    public required Cursor<IToken> TokensCursor { get; init; }
}

public class LangParser
{
    public static Option<ILangTree> Parse(ILangRoot syntaxRoot, IReadOnlyList<IToken> tokens)
    {
        syntaxRoot.NotNull();
        tokens.NotNull();

        var pContext = new LangParserContext
        {
            SyntaxCurrent = syntaxRoot,
            WriteNode = new LangTree(),
            TokensCursor = tokens.ToCursor(),
        };

        while (pContext.TokensCursor.TryNextValue(out var token))
        {
            if (!pContext.GetCursor().TryNextValue(out var syntax)) return (StatusCode.BadRequest, "no syntax");

            Option<string> state = syntax.Check(token);
            if (state.IsError()) return (StatusCode.BadRequest, $"Syntax error, token={token.Value}");

            pContext.WriteNode.Add(syntax, state.Return());
        }

        return pContext.WriteNode.ToOption();
    }
}

using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("Name={Name}")]
public class LsRepeat : LsRoot, ILangSyntax
{
    public LsRepeat(string? name) => Name = name;

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax> _)
    {
        bool first = true;
        var nodes = new LangNodes();

        bool isDelimter = false;

        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            if (!first && !isDelimter) break;

            var result = this.MatchSyntaxSegement(pContext);
            if (result.IsError()) break;

            LangNodes langNodes = result.Return();
            isDelimter = IsDelimiter(pContext, langNodes);

            nodes += result.Return();
            first = false;
        }

        if (first) return (StatusCode.BadRequest, "Syntax error, no repeating values");

        return nodes;
    }

    private bool IsDelimiter(LangParserContext pContext, LangNodes ln) => Children.Count > 0 &&
        Children.Count == ln.Children.Count &&
        Children.Last() is LsToken token &&
        token.Symbol == ln.Last().Value;

    public static LsRepeat operator +(LsRepeat subject, ILangSyntax value) => subject.Action(x => x.Children.Add(value));
    public static LsRepeat operator +(LsRepeat subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsRepeat operator +(LsRepeat subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}

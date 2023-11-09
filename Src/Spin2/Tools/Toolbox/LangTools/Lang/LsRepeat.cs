using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("Name={Name}")]
public class LsRepeat : LangBase<ILangSyntax>, ILangRoot
{
    public LsRepeat(string? name) => Name = name;

    public LsRepeat(bool noDelimiter, string? name) => (NoDelimiter, Name) = (noDelimiter, name);

    public string? Name { get; }
    public bool NoDelimiter { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _)
    {
        var nodes = new LangNodes();
        bool first = true;
        bool isDelimter = false;

        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
             if (!NoDelimiter && !first && !isDelimter) break;

            var result = this.MatchSyntaxSegement(nameof(LsRepeat), pContext);
            if (result.IsError()) break;

            LangNodes langNodes = result.Return();
            if (!NoDelimiter) isDelimter = IsDelimiter(pContext, langNodes);

            nodes += result.Return();
            first = false;
        }

        if (first) return (StatusCode.BadRequest, "Syntax error, no repeating values");

        return nodes;
    }

    private bool IsDelimiter(LangParserContext pContext, LangNodes ln) => Children.Count > 0 &&
        ln.Children.Count > 0 &&
        Children.Last() is LsToken token &&
        token.Token == ln.Last().Value;

    public override string ToString() => $"{nameof(LsRepeat)}: Name={Name}, Syntax=[ {this.Select(x => x.ToString()).Join(' ')} ]";

    public static LsRepeat operator +(LsRepeat subject, ILangSyntax value) => subject.Action(x => x.Children.Add(value));
    public static LsRepeat operator +(LsRepeat subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsRepeat operator +(LsRepeat subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}

using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("Name={Name}")]
public class LsOr : LsRoot, ILangSyntax
{
    public LsOr(string? name) => Name = name;

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax> _)
    {
        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            var result = this.MatchSyntaxSegement(pContext);
            if (result.IsOk()) return result;
        }

        return (StatusCode.BadRequest, "Syntax error, no repeating values");
    }

    public static LsOr operator +(LsOr subject, ILangSyntax value)
    {
        //var root = new LsRoot() + value;
        subject.Children.Add(value);
        return subject;
    }
    public static LsOr operator +(LsOr subject, LsRoot root) => subject.Action(x => x.Children.AddRange(root));
    public static LsOr operator +(LsOr subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsOr operator +(LsOr subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}

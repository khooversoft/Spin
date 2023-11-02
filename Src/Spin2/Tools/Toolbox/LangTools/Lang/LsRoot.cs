using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


public interface ILangSyntax
{
    string? Name { get; }
    Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? syntaxCursor = null);
}


public interface ILangRoot : ILangBase<ILangSyntax>, ILangSyntax
{
}

public class LsRoot : LangBase<ILangSyntax>, ILangRoot
{
    public LsRoot(string? name = null) => Name = name;
    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _)
    {
        Option<LangNodes> nodes = this.MatchSyntaxSegement(nameof(LsRoot), pContext);
        return nodes;
    }

    public override string ToString() => $"{nameof(LsRoot)}: Name={Name}, Syntax=[ {this.Select(x => x.ToString()).Join(' ')} ]";

    public static LsRoot operator +(LsRoot subject, ILangSyntax value) => subject.Action(x => x.Children.Add(value));
    public static LsRoot operator +(LsRoot subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsRoot operator +(LsRoot subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}

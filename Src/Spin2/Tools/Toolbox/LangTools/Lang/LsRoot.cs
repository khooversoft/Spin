using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


public interface ILangRoot : ILangBase<ILangSyntax>
{
}

public interface ILangSyntax
{
    string? Name { get; }
    Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax> syntaxCursor);
}


public class LsRoot : LangBase<ILangSyntax>, ILangRoot
{
    public static LsRoot operator +(LsRoot subject, ILangSyntax value) => subject.Action(x => x.Children.Add(value));
    public static LsRoot operator +(LsRoot subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsRoot operator +(LsRoot subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}

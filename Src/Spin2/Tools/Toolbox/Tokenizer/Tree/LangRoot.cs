using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Tokenizer.Tree;


public interface ILangRoot : ILangBase<ILangSyntax>
{
}

public class LangRoot : LangBase<ILangSyntax>, ILangRoot
{
    public LangRoot() { }
    protected LangRoot(LangType type) : base(type) { }
}


public static class LangRootExtensions
{
    public static ILangRoot AddValue(this ILangRoot subject, string? name = null)
    {
        subject.NotNull();

        var newNode = new LangSyntaxValue(subject, name: name);
        subject.Children.Add(newNode);

        return subject;
    }

    public static ILangRoot AddSyntax(this ILangRoot subject, string symbol, string? name = null)
    {
        subject.NotNull();
        symbol.NotEmpty();

        var newNode = new LangSyntaxToken(subject, symbol: symbol, name: name);
        subject.Children.Add(newNode);

        return subject;
    }
}
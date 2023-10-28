using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Tokenizer.Tree;


public interface ILangSyntax : ILangRoot
{
    ILangRoot Parent { get; }
    string? Name { get; }
    Option<string> Check(IToken token);
}


public class LangSyntaxValue : LangRoot, ILangSyntax
{
    public LangSyntaxValue(ILangRoot parent, string? name)
        : base(LangType.Value)
    {
        Parent = parent.NotNull();
        Name = name;
    }

    public ILangRoot Parent { get; }
    public string? Name { get; }

    public Option<string> Check(IToken token)
    {
        Option<string> result = token switch
        {
            TokenValue tokenValue when !tokenValue.IsSyntaxToken => tokenValue.Value,
            _ => StatusCode.NotFound,
        };

        return result;
    }
}


public class LangSyntaxToken : LangRoot, ILangSyntax
{
    public LangSyntaxToken(ILangRoot parent, string symbol, string? name)
        : base(LangType.SyntaxToken)
    {
        Parent = parent.NotNull();
        Symbol = symbol.NotNull();
        Name = name;
    }

    public ILangRoot Parent { get; }
    public string Symbol { get; }
    public string? Name { get; }

    public Option<string> Check(IToken token)
    {
        Option<string> result = token switch
        {
            TokenValue tokenValue when tokenValue.IsSyntaxToken && tokenValue.Value == Symbol => tokenValue.Value,
            _ => StatusCode.NotFound,
        };

        return result;
    }
}

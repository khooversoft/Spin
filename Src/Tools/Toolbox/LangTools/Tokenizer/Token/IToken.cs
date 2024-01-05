namespace Toolbox.LangTools;

public enum TokenType
{
    None,
    Token,
    Block,
    Unicode,
}

public interface IToken
{
    string Value { get; }
    TokenType TokenType { get; }
}
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class InterContext
{
    public InterContext(IEnumerable<SyntaxPair> syntaxPairs) => Cursor = new List<SyntaxPair>(syntaxPairs.NotNull()).ToCursor();

    public Cursor<SyntaxPair> Cursor { get; }
}

internal static class InterContextExtensions
{
    public static Option ProcessSymbols(this InterContext ic, params string[] symbolNames)
    {
        foreach (var symbol in symbolNames)
        {
            if (!ic.Cursor.TryGetValue(out var token) || token.Name != symbol)
            {
                return (StatusCode.NotFound, $"Expected symbol '{symbol}' not found");
            }
        }

        return StatusCode.OK;
    }

    public static Option IsSymbol(this InterContext ic, string symbolName)
    {
        if (ic.Cursor.TryPeekValue(out var token) && token.Name == symbolName)
        {
            ic.Cursor.MoveNext();
            return StatusCode.OK;
        }

        return StatusCode.NotFound;
    }

    public static Option<string> IsSymbol(this InterContext ic, params string[] symbolNames)
    {
        foreach (var symbolName in symbolNames.NotNull())
        {
            if (ic.Cursor.TryPeekValue(out var token) && token.Name == symbolName)
            {
                ic.Cursor.MoveNext();
                return token.Name;
            }
        }

        return StatusCode.NotFound;
    }

    public static Option<string> GetValue(this InterContext ic, string expectedName, bool optional = false)
    {
        SyntaxPair? token;

        if (optional)
        {
            if (ic.Cursor.TryPeekValue(out token) && token.Name == expectedName)
            {
                ic.Cursor.MoveNext();
                return token.Token.Value;
            }

            return StatusCode.NotFound;
        }

        if (ic.Cursor.TryGetValue(out token) && token.Name == expectedName) return token.Token.Value;

        return (StatusCode.NotFound, $"Expected token '{expectedName}' not found");
    }

    public static Option<T> GetEnum<T>(this InterContext ic, params string[] expectedNames) where T : struct, Enum
    {
        if (!ic.Cursor.TryGetValue(out var token) || !expectedNames.Contains(token.Name))
        {
            return (StatusCode.NotFound, $"Expected token '{expectedNames.Join(',')}' not found");
        }

        if (!token.Token.Value.TryToEnum<T>(out var enumValue, true)) return (StatusCode.BadRequest, $"Invalid enum for {token.Token.Value}");
        return enumValue;
    }
}

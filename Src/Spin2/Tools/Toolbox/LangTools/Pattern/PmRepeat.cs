using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PmRepeat : PatternBase
{
    public PmRepeat(string delimiter, string? name = null) : base(name)
    {
        Delimiter = delimiter;
    }

    public string Delimiter { get; }

    public override Option<Sequence<IPatternSyntax>> Process(PatternContext pContext)
    {
        var nodes = new Sequence<IPatternSyntax>();
        bool first = true;

        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            if (!first && token is TokenValue tokenValue && tokenValue.Value == Delimiter) break;
            first = false;

            if (!pContext.Syntax.TryNextValue(out IPatternSyntax? syntax)) break;

            var result = syntax.MatchSyntaxSegement(pContext);
            if (result.IsError()) break;

            nodes += result.Return();
        }

        if (first) return (StatusCode.BadRequest, "Syntax error, no repeating values");

        return nodes;
    }
}

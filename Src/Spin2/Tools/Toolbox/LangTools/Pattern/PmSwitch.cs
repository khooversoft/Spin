using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PmSwitch : PatternBase
{
    public PmSwitch(string? name = null) : base(name) { }

    public override Option<Sequence<IPatternSyntax>> Process(PatternContext pContext)
    {
        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            foreach (var item in Children)
            {
                var result = item.MatchSyntaxSegement(pContext);
                if (result.IsOk()) return result;
            }
        }

        return (StatusCode.BadRequest, $"Option={Name} failed");
    }
}
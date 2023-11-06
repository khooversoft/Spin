using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PmOption : PatternBase
{
    public PmOption(string? name = null) : base(name) { }

    public override Option<Sequence<IPatternSyntax>> Process(PatternContext pContext)
    {
        if (!pContext.Syntax.TryNextValue(out _)) return (StatusCode.BadRequest, "Syntax cursor is empty");
        pContext.Push();

        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            foreach (var item in Children)
            {
                var result = item.MatchSyntaxSegement(pContext);
                if (result.IsOk())
                {
                    pContext.RemovePush();
                    return result;
                }
            }
        }

        pContext.Pop();
        return StatusCode.NoContent;
    }

    public static PmOption operator +(PmOption subject, IPatternSyntax syntax) => subject.Action(x => x.Add(syntax));
}

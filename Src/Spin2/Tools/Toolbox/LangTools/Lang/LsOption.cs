using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("Name={Name}")]
public class LsOption : LangBase<ILangSyntax>, ILangRoot
{
    public LsOption(string? name) => Name = name;

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _)
    {
        var result = pContext.RunAndLog(nameof(LsOption), Name, () => InternalProcess(pContext));
        return result;
    }

    private Option<LangNodes> InternalProcess(LangParserContext pContext)
    {
        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            foreach (var item in Children.OfType<ILangRoot>())
            {
                var result = item.MatchSyntaxSegement(pContext);
                pContext.Log(nameof(LsOption) + ":MatchSyntaxSegement", result, Name);
                if (result.IsOk()) return result;
            }

            break;
        }

        return (StatusCode.BadRequest, "Syntax error, no repeating values");
    }

    public override string ToString() => $"{nameof(LsOption)}: Name={Name}, nodes=[ {this.Select(x => x.ToString()).Join(' ')} ]";

    public static LsOption operator +(LsOption subject, ILangRoot syntax) => subject.Action(x => x.Children.Add(syntax));
}

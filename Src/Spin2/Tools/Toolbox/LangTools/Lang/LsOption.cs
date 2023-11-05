using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("Name={Name}")]
public class LsOption : LangBase<ILangSyntax>, ILangRoot
{
    public LsOption(string? name = null) => Name = name;

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _)
    {
        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            foreach (var item in Children.OfType<ILangRoot>())
            {
                var result = item.MatchSyntaxSegement(nameof(LsOption), pContext);
                if (result.IsOk()) return result;
            }

            break;
        }

        return (StatusCode.BadRequest, "Syntax error, no repeating values");
    }

    public override string ToString() => $"{nameof(LsOption)}: Name={Name}, nodes=[ {this.Select(x => x.ToString()).Join(' ')} ]";

    public static LsOption operator +(LsOption subject, ILangRoot syntax) => subject.Action(x => x.Children.Add(syntax));
}

using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("Name={Name}")]
public class LsSwitch : LangBase<ILangSyntax>, ILangRoot
{
    public LsSwitch(string? name = null) => Name = name;

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _)
    {
        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            foreach (var item in Children)
            {
                switch (item)
                {
                    case ILangRoot root:
                        var result = root.MatchSyntaxSegement(nameof(LsSwitch), pContext);
                        if (result.IsOk()) return result;
                        break;

                    case ILangSyntax syntax:
                        var syntaxCursor = syntax.CreateCursor();

                        var result2 = syntax.Process(pContext, syntaxCursor);
                        if (result2.IsOk()) return result2;

                        pContext.TokensCursor.Index--;
                        break;
                }
            }

            break;
        }

        return (StatusCode.BadRequest, "Syntax error, switch failed");
    }

    public override string ToString() => $"{nameof(LsSwitch)}: Name={Name}, nodes=[ {this.Select(x => x.ToString()).Join(' ')} ]";

    public static LsSwitch operator +(LsSwitch subject, ILangRoot syntax) => subject.Action(x => x.Children.Add(syntax));
    public static LsSwitch operator +(LsSwitch subject, ILangSyntax syntax) => subject.Action(x => x.Children.Add(syntax));
}

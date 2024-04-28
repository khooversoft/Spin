using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


// Same as Switch, need to re-work
[DebuggerDisplay("Name={Name}")]
public class LsOption : LangBase<ILangSyntax>, ILangRoot
{
    public LsOption(string? name = null) => Name = name;

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _ = null)
    {
        var nodes = new LangNodes();

        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            foreach (var item in Children)
            {
                switch (item)
                {
                    case ILangRoot root:
                        var result = root.MatchSyntaxSegement(nameof(LsOption), pContext);
                        if (result.IsError()) return new LangNodes();

                        nodes += result.Return();
                        break;

                    case ILangSyntax syntax:
                        var syntaxCursor = syntax.CreateCursor();

                        var result2 = syntax.Process(pContext, syntaxCursor);
                        if (result2.IsError())
                        {
                            pContext.TokensCursor.Index--;
                            return new LangNodes();
                        }

                        nodes += result2.Return();
                        break;
                }
            }

            break;
        }

        return nodes;
    }

    public override string ToString() => $"{nameof(LsOption)}: Name={Name}, nodes=[ {this.Select(x => x.ToString()).Join(' ')} ]";

    public static LsOption operator +(LsOption subject, ILangRoot syntax) => subject.Action(x => x.Children.Add(syntax));
    public static LsOption operator +(LsOption subject, ILangSyntax syntax) => subject.Action(x => x.Children.Add(syntax));
}

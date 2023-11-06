using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public interface IPatternSyntax /*: IPatternBase<IPatternSyntax>*/
{
    string? Name { get; }
    Option<Sequence<IPatternSyntax>> Process(PatternContext pContext);
}

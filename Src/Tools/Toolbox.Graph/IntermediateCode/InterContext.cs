using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal class InterContext
{
    public InterContext(IEnumerable<SyntaxPair> syntaxPairs) => Cursor = new List<SyntaxPair>(syntaxPairs.NotNull()).ToCursor();

    public Cursor<SyntaxPair> Cursor { get; }
}

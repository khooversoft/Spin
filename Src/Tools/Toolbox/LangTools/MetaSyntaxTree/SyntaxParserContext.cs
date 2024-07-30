using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class SyntaxParserContext
{
    private readonly Stack<int> _postionStack = new Stack<int>();
    public SyntaxParserContext(IReadOnlyList<IToken> tokens) => TokensCursor = new Cursor<IToken>(tokens.ToImmutableArray());

    public Cursor<IToken> TokensCursor { get; }

    public FinalizeScope<SyntaxParserContext> PushWithScope()
    {
        Push();
        return new FinalizeScope<SyntaxParserContext>(this, x => x.Pop(), x => RemovePush());
    }

    public void Push() => _postionStack.Push(TokensCursor.Index);

    public void Pop()
    {
        _postionStack.TryPop(out var position).Assert(x => x == true, "Empty stack");
        TokensCursor.Index = position;
    }

    public void RemovePush() => _postionStack.TryPop(out var _).Assert(x => x == true, "Empty stack");
}

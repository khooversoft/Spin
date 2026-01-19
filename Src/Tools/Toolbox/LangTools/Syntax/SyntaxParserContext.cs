using System.Collections.Immutable;
using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class SyntaxParserContext
{
    private readonly Stack<int> _position = new Stack<int>();

    public SyntaxParserContext(IReadOnlyList<IToken> tokens)
    {
        TokensCursor = new Cursor<IToken>(tokens.ToImmutableArray());
    }

    public Cursor<IToken> TokensCursor { get; }
    public SyntaxTreeBuilder SyntaxTree { get; } = new SyntaxTreeBuilder();

    public FinalizeScope<SyntaxParserContext> PushWithScope()
    {
        Push();
        return new FinalizeScope<SyntaxParserContext>(this, x => x.Pop(), x => RemovePush());
    }

    public void Push() => _position.Push(TokensCursor.Index);

    public void Pop()
    {
        _position.TryPop(out var position).Assert(x => x == true, "Empty stack");
        TokensCursor.Index = position;
    }

    public void RemovePush() => _position.TryPop(out var _).Assert(x => x == true, "Empty stack");

    public string GetDebuggerDisplay() => $"PostionStack.Count={_position.Count}, TokensCursor={TokensCursor.GetDebuggerDisplay()}";
}

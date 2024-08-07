using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class SyntaxParserContext
{
    private readonly Stack<int> _postionStack = new Stack<int>();
    private readonly ScopeContext _context;

    public SyntaxParserContext(IReadOnlyList<IToken> tokens, ScopeContext context)
    {
        TokensCursor = new Cursor<IToken>(tokens.ToImmutableArray());
        _context = context;
    }

    public Cursor<IToken> TokensCursor { get; }
    public SyntaxTreeBuilder SyntaxTree { get; } = new SyntaxTreeBuilder();

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

    public string GetDebuggerDisplay(bool newLine = false) => new string[]
    {
        $"PostionStack.Count={_postionStack.Count}",
        $"TokensCursor={TokensCursor.GetDebuggerDisplay()}",
    }.Join(newLine ? Environment.NewLine : ", ");
}

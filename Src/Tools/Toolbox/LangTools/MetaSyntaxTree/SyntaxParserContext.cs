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
    private readonly Stack<Position> _postionStack = new Stack<Position>();
    private readonly ScopeContext _context;

    public SyntaxParserContext(IReadOnlyList<IToken> tokens, ScopeContext context)
    {
        TokensCursor = new Cursor<IToken>(tokens.ToImmutableArray());
        _context = context;
    }

    public Cursor<IToken> TokensCursor { get; }
    public Sequence<SyntaxPair> Pairs { get; private set; } = new Sequence<SyntaxPair>();

    public FinalizeScope<SyntaxParserContext> PushWithScope()
    {
        Push();
        return new FinalizeScope<SyntaxParserContext>(this, x => x.Pop(), x => RemovePush());
    }

    public void Push()
    {
        _context.LogInformation("SyntaxParserContext: Pushing position, TokensCursor.Index={tokensCursorIndex}, Pairs.Count={pairsCount}", TokensCursor.Index, Pairs.Count);
        _postionStack.Push(new Position(TokensCursor.Index, Pairs));
    }

    public void Pop()
    {
        _context.LogInformation("SyntaxParserContext: Pop position, TokensCursor.Index={tokensCursorIndex}, Pairs.Count={pairsCount}", TokensCursor.Index, Pairs.Count);
        _postionStack.TryPop(out var position).Assert(x => x == true, "Empty stack");
        TokensCursor.Index = position.TokenCursorIndex;
        Pairs = position.Pairs;
    }

    public void RemovePush()
    {
        _context.LogInformation("SyntaxParserContext: Removing Push position, TokensCursor.Index={tokensCursorIndex}, Pairs.Count={pairsCount}", TokensCursor.Index, Pairs.Count);
        _postionStack.TryPop(out var _).Assert(x => x == true, "Empty stack");
    }

    private readonly struct Position
    {
        public Position(int tokenCursorIndex, IEnumerable<SyntaxPair> pairs)
        {
            TokenCursorIndex = tokenCursorIndex;
            Pairs = pairs.ToSequence();
        }

        public int TokenCursorIndex { get; }
        public Sequence<SyntaxPair> Pairs { get; }
    }

    public string GetDebuggerDisplay(bool newLine = false) => new string[]
    {
        $"PostionStack.Count={_postionStack.Count}",
        $"TokensCursor={TokensCursor.GetDebuggerDisplay()}",
        $"Pairs={Pairs.Count}",
    }.Join(newLine ? Environment.NewLine : ", ");
}

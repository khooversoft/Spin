using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class LangParserContext
{
    private readonly Stack<Position> _postionStack = new Stack<Position>();

    public LangParserContext(ILangRoot root, IReadOnlyList<IToken> tokens)
    {
        Root = root;
        TokensCursor = tokens.ToCursor();

        Symbols = root.GetChildrenRecursive()
            .OfType<LsSymbol>()
            .ToDictionary(x => x.Symbol, x => x, StringComparer.OrdinalIgnoreCase);
    }

    public ILangRoot Root { get; private set; }
    public Cursor<IToken> TokensCursor { get; }
    public IReadOnlyDictionary<string, LsSymbol> Symbols { get; }

    public FinalizeScope<LangParserContext> PushWithScope(ILangRoot langRoot)
    {
        Push(langRoot);
        return new FinalizeScope<LangParserContext>(this, x => x.Pop());
    }

    public void Push(ILangRoot langRoot)
    {
        var postion = new Position
        {
            Root = Root,
            TokensCursorIndex = TokensCursor.Index,
        };

        _postionStack.Push(postion);

        Root = langRoot.NotNull();
    }

    public void Pop()
    {
        _postionStack.Count.Assert(x => x > 0, "Stack empty");

        var position = _postionStack.Pop();
        Root = position.Root;
        TokensCursor.Index = position.TokensCursorIndex;
    }

    public void RemovePush()
    {
        if (_postionStack.Count > 0) _postionStack.Pop();
    }

    private readonly record struct Position
    {
        public ILangRoot Root { get; init; }
        public int TokensCursorIndex { get; init; }
    }
}

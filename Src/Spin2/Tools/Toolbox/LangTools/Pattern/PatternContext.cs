using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PatternContext
{
    private readonly Stack<int> _tokenIndex = new Stack<int>();
    private readonly Stack<Cursor<IPatternSyntax>> _syntaxCursor = new Stack<Cursor<IPatternSyntax>>();

    public PatternContext(IReadOnlyList<IToken> tokens) => TokensCursor = tokens.ToCursor();

    public Cursor<IPatternSyntax> Syntax => _syntaxCursor.Peek();
    public Cursor<IToken> TokensCursor { get; }
    public List<LangTrace> Trace { get; } = new List<LangTrace>();

    public void Push()
    {
        _tokenIndex.Push(TokensCursor.Index);
        _syntaxCursor.Push(Syntax);
    }

    public void Push(IPatternSyntax syntax)
    {
        _tokenIndex.Push(TokensCursor.Index);

        switch (syntax)
        {
            case IPatternBase<IPatternSyntax> collection:
                _syntaxCursor.Push(new Cursor<IPatternSyntax>(collection.Children));
                break;

            case IPatternSyntax:
                _syntaxCursor.Push(new Cursor<IPatternSyntax>(new[] { syntax }));
                break;

            default: throw new UnreachableException();
        }
    }

    public void Pop()
    {
        _tokenIndex.Count.Assert(x => x > 0, "Stack empty");

        TokensCursor.Index = _tokenIndex.Pop();
        _syntaxCursor.Pop();
    }

    public void RemovePush()
    {
        if (_tokenIndex.Count > 0)
        {
            _tokenIndex.Pop();
            _syntaxCursor.Pop();
        }
    }
}

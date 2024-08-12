﻿using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class MetaParserContext
{
    private readonly Stack<int> _postionStack = new Stack<int>();

    public MetaParserContext(IEnumerable<IToken> tokens) => TokensCursor = new Cursor<IToken>(tokens.ToImmutableArray());

    public Cursor<IToken> TokensCursor { get; }
    public ProductionRuleBuilder RootRule { get; } = new ProductionRuleBuilder { Name = "Root", Type = ProductionRuleType.Root };
    public Dictionary<string, IMetaSyntax> Nodes { get; init; } = new Dictionary<string, IMetaSyntax>(StringComparer.OrdinalIgnoreCase);

    public void Add(IMetaSyntax syntax)
    {
        syntax.NotNull();
        RootRule.NotNull();

        Nodes.TryAdd(syntax.Name, syntax).Assert(x => x == true, $"Syntax node '{syntax.Name}' already exists");
        RootRule.Children.Add(syntax);
    }

    public FinalizeScope<MetaParserContext> PushWithScope()
    {
        Push();
        return new FinalizeScope<MetaParserContext>(this, x => x.Pop(), x => RemovePush());
    }

    public void Push() => _postionStack.Push(TokensCursor.Index);

    public void Pop()
    {
        _postionStack.TryPop(out var position).Assert(x => x == true, "Empty stack");
        TokensCursor.Index = position;
    }

    public void RemovePush() => _postionStack.TryPop(out var _).Assert(x => x == true, "Empty stack");
}

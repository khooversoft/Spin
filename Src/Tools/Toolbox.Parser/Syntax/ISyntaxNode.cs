using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Parser.Syntax;


public interface ISyntaxNode
{
    ISyntaxNodeCollection? Parent { get; set; }
}


public interface ISyntaxNodeCollection : IEnumerable<ISyntaxNode>
{
    int Count { get; }

    void Add(ISyntaxNode value);
    void AddRange(IEnumerable<ISyntaxNode> values);
}



public static class ISyntaxNodeExtensions
{
    public static ISyntaxNode SetParent(this ISyntaxNode subject, ISyntaxNodeCollection? parent) => subject
        .NotNull()
        .Action(x => x.Parent = parent.NotNull());
}

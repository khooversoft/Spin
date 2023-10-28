using Toolbox.Tools;

namespace Toolbox.Tokenizer.Tree;

public interface ILangTree : ILangBase<LangNode>
{
}

public class LangTree : LangBase<LangNode>, ILangTree
{
    public LangTree() { }
    protected LangTree(LangType type) : base(type) { }
}


public static class LangTreeExtensions
{
    public static ILangTree Add(this ILangTree subject, ILangSyntax syntaxNode, string? value = null)
    {
        subject.NotNull();

        var newNode = new LangNode(subject, syntaxNode, value);
        subject.Children.Add(newNode);

        return subject;
    }
}
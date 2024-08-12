﻿namespace Toolbox.LangTools;

public interface ISyntaxTree
{
}


public sealed record SyntaxTree : ISyntaxTree
{
    public string? MetaSyntaxName { get; init; }
    public IReadOnlyList<ISyntaxTree> Children { get; init; } = Array.Empty<ISyntaxTree>();

    public bool Equals(SyntaxTree? obj)
    {
        bool result = obj is SyntaxTree subject &&
            (MetaSyntaxName, subject.MetaSyntaxName) switch
            {
                (null, null) => true,
                (string v1, string v2) => v1.Equals(v2),
                _ => false,
            } &&
            Enumerable.SequenceEqual(Children, subject.Children);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(MetaSyntaxName, Children);
}

﻿using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;

public enum ProductionRuleType
{
    Root,
    Group,
    Repeat,
    Optional,
}

public enum EvaluationType
{
    None,
    Sequence,
    Or,
}

[DebuggerDisplay("ProductionRule: Name={Name}, Type={Type}, Children.Count={Children.Count}, Index={Index}")]
public sealed record ProductionRule : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public ProductionRuleType Type { get; init; } = ProductionRuleType.Root;
    public EvaluationType EvaluationType { get; set; } = EvaluationType.Sequence;
    public Sequence<IMetaSyntax> Children { get; init; } = new Sequence<IMetaSyntax>();
    public int? Index { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    public bool Equals(ProductionRule? obj)
    {
        bool result = obj is ProductionRule subject &&
            Name == subject.Name &&
            Type == subject.Type &&
            EvaluationType == subject.EvaluationType &&
            Enumerable.SequenceEqual(Children, subject.Children) &&
            Enumerable.SequenceEqual(Tags.OrderBy(x => x), subject.Tags.OrderBy(x => x));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Type, Children);

    public override string ToString() => $"ProductionRule [ Name={Name}, Type={Type}, Index={Index}, ChildrenCount={Children.Count} ]".ToEnumerable()
        .Concat(Children.Select(x => x.ToString()))
        .Join(Environment.NewLine);

    public IEnumerable<T> GetAll<T>() where T : IMetaSyntax
    {
        foreach (var item in Children)
        {
            if (item is T resolved) yield return resolved;

            if (item is ProductionRule rule)
            {
                foreach (var pr in rule.GetAll<T>()) yield return pr;
            }
        }
    }
}

[DebuggerDisplay("ProductionRuleReference: Name={Name}, Index={Index}")]
public sealed record ProductionRuleReference : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string ReferenceSyntax { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(ProductionRuleReference? obj) => obj is ProductionRuleReference subject &&
        Name == subject.Name &&
        ReferenceSyntax == subject.ReferenceSyntax;

    public override int GetHashCode() => HashCode.Combine(Name);
    public override string ToString() => $"ProductionRuleReference [ Name={Name}, Index={Index} ]";
}

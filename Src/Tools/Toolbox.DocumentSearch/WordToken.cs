using System.Diagnostics.CodeAnalysis;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.DocumentSearch;

public record WordToken
{
    public WordToken() { }

    public WordToken(string word, int weight) => (Word, Weight) = (word.NotEmpty(), weight);
    public string Word { get; init; } = null!;
    public int Weight { get; init; }

    public static IValidator<WordToken> Validator { get; } = new Validator<WordToken>()
        .RuleFor(x => x.Word).NotEmpty()
        .RuleFor(x => x.Weight).Must(x => x >= 0, x => $"Invalid weight {x}")
        .Build();
}

public static class WordTokenExtensions
{
    public static Option Validate(this WordToken subject) => WordToken.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this WordToken subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}


public class WordTokenComparer : IEqualityComparer<WordToken>
{
    public static WordTokenComparer Instance { get; } = new WordTokenComparer();

    public bool Equals(WordToken? x, WordToken? y)
    {
        if (x == null || y == null) return false;

        return x.Word.EqualsIgnoreCase(y.Word) &&
            x.Weight == y.Weight;
    }

    public int GetHashCode([DisallowNull] WordToken obj) => HashCode.Combine(obj.Word);
}
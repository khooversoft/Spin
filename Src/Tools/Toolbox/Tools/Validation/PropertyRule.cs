using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public interface IPropertyRuleBase<T>
{
    Option<IValidatorResult> Validate(T value);
}

public interface IPropertyValidator<T, TProperty>
{
    Option<IValidatorResult> Validate(T subject, TProperty property);
}

public interface IPropertyRule<T, TProperty> : IPropertyRuleBase<T>
{
    string Name { get; }
    IList<IPropertyValidator<T, TProperty>> Validators { get; }
}


public record PropertyRule<T, TProperty> : IPropertyRule<T, TProperty>
{
    public string Name { get; init; } = null!;
    public Func<T, TProperty> GetValue { get; init; } = null!;
    public IList<IPropertyValidator<T, TProperty>> Validators { get; init; } = new List<IPropertyValidator<T, TProperty>>();

    public Option<IValidatorResult> Validate(T subject)
    {
        TProperty property = GetValue(subject);

        return Validators
            .Select(x => x.Validate(subject, property))
            .Where(x => x.HasValue)
            .Select(x => x.Return())
            .FirstOrDefaultOption();
    }
};

public record PropertyCollectionRule<T, TProperty> : IPropertyRule<T, TProperty>
{
    public string Name { get; init; } = null!;
    public Func<T, IEnumerable<TProperty>> GetValue { get; init; } = null!;
    public IList<IPropertyValidator<T, TProperty>> Validators { get; init; } = new List<IPropertyValidator<T, TProperty>>();

    public Option<IValidatorResult> Validate(T subject)
    {
        IEnumerable<TProperty> properties = GetValue(subject);

        var list = new List<IValidatorResult>();

        foreach (var item in properties)
        {
            var result = Validators
                .Select(x => x.Validate(subject, item))
                .Where(x => x.HasValue)
                .Select(x => x.Return())
                .FirstOrDefaultOption();

            if (result.HasValue) list.Add(result.Return());
        }

        return new ValidatorResult
        {
            Errors = list.ToArray(),
        };
    }
};

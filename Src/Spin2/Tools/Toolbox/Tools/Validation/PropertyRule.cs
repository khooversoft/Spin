using System.ComponentModel.DataAnnotations;
using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation;

public interface IPropertyRuleBase<T>
{
    Option<IValidateResult> Validate(T value);
}

public interface IPropertyRule<T, TProperty> : IPropertyRuleBase<T>
{
    string Name { get; }
    IList<IValidator<TProperty>> Validators { get; }
}


public record PropertyRule<T, TProperty> : IPropertyRule<T, TProperty>
{
    public string Name { get; init; } = null!;
    public Func<T, TProperty> GetValue { get; init; } = null!;
    public IList<IValidator<TProperty>> Validators { get; init; } = new List<IValidator<TProperty>>();

    public Option<IValidateResult> Validate(T value)
    {
        TProperty property = GetValue(value);

        return Validators
            .Select(x => x.Validate(property))
            .Where(x => x.HasValue)
            .Select(x => x.Return())
            .FirstOrDefaultOption();
    }
};

public record PropertyCollectionRule<T, TProperty> : IPropertyRule<T, TProperty>
{
    public string Name { get; init; } = null!;
    public Func<T, IEnumerable<TProperty>> GetValue { get; init; } = null!;
    public IList<IValidator<TProperty>> Validators { get; init; } = new List<IValidator<TProperty>>();

    public Option<IValidateResult> Validate(T value)
    {
        IEnumerable<TProperty> properties = GetValue(value);

        var list = new List<IValidateResult>();

        foreach (var item in properties)
        {
            var result = Validators
                .Select(x => x.Validate(item))
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




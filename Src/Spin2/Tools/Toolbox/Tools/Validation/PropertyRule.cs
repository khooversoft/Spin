using Toolbox.Tools.Validation.Validators;
using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation;

public interface IPropertyRuleBase<T>
{
    Option<IValidateResult> Validate(T value);
}

public interface IPropertyRule<T, TProperty> : IPropertyRuleBase<T>
{
    string Name { get; }
    Func<T, TProperty> GetValue { get; }
    IList<IValidator<T>> Validators { get; }
}


public record PropertyRule<T, TProperty> : IPropertyRule<T, TProperty>
{
    public string Name { get; init; } = null!;
    public Func<T, TProperty> GetValue { get; init; } = null!;
    public IList<IValidator<T>> Validators { get; init; } = new List<IValidator<T>>();

    public Option<IValidateResult> Validate(T value)
    {
        return Validators
            .Select(x => x.Validate(value))
            .Where(x => x.HasValue)
            .Select(x => x.Return())
            .FirstOrDefaultOption();
    }
};


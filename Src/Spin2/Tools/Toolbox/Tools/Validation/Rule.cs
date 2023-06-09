using Toolbox.Tools.Validation.Validators;

namespace Toolbox.Tools.Validation;

public readonly struct Rule<T, TProperty>
{
    public required Validator<T> Validator { get; init; }
    public required IPropertyRule<T, TProperty> PropertyRule { get; init; }

    public Rule<T, TProperty> NotNull(string errorMessage = "is required")
    {
        PropertyRule.Validators.Add(new NotNull<T, TProperty>(PropertyRule, errorMessage));
        return this;
    }
}
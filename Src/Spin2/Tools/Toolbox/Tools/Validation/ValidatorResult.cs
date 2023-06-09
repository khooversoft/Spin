using Toolbox.Extensions;
using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation;

public interface IValidateResult { }

public record ValidatorResult<T> : IValidateResult
{
    public IReadOnlyList<ValidatorError> Errors { get; init; } = Array.Empty<ValidatorError>();

    public bool IsValid => Errors.Count == 0;

    public T? Subject { get; set; }

    public T Verify()
    {
        if (IsValid) return Subject.NotNull();

        string msg = "Property errors: " + Errors.Select(x => x.ToString()).Join(", ");
        throw new ArgumentException(msg);
    }
}


public record ValidatorError : IValidateResult
{
    public string Name { get; init; } = null!;
    public string Message { get; init; } = null!;

    public override string ToString() => $"Property {Name}, {Message}";
}


public static class ValidationErrorExtensions
{
    public static Option<IValidateResult> CreateError<T, TProperty>(this IPropertyRule<T, TProperty> propertyRule, string message)
    {
        propertyRule.NotNull();
        message.NotEmpty();

        return new ValidatorError
        {
            Name = propertyRule.Name,
            Message = message,
        };
    }
}
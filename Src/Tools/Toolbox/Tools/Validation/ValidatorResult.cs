using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public interface IValidatorResult { }

public record ValidatorResult : IValidatorResult
{
    public IReadOnlyList<IValidatorResult> Errors { get; init; } = Array.Empty<IValidatorResult>();

    public bool IsValid => Errors.Count == 0;

    public override string ToString() => Errors.NotNull().Select(x => x.ToString()).Join(", ");
}


public static class ValidationErrorExtensions
{
    public static Option<IValidatorResult> CreateError<T, TProperty>(this IPropertyRule<T, TProperty> propertyRule, string message)
    {
        propertyRule.NotNull();
        message.NotEmpty();

        return new ValidatorError
        {
            TypeName = typeof(T).GetTypeName(),
            Name = propertyRule.Name,
            Message = message,
        };
    }

    public static IReadOnlyList<ValidatorError> GetErrors(this ValidatorResult subject) => subject.Errors
        .SelectMany(x => x switch
        {
            ValidatorError v => new[] { v },
            ValidatorResult v => v.GetErrors(),

            var v => throw new InvalidOperationException($"Invalid IValidateResult class, type={v.GetType().FullName}"),
        })
        .ToArray();
}

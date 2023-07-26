using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools.Validation;

public interface IValidatorResult { }

public record ValidatorResult : IValidatorResult
{
    public IReadOnlyList<IValidatorResult> Errors { get; init; } = Array.Empty<IValidatorResult>();

    public bool IsValid => Errors.Count == 0;

    public Option ToOption() => new Option(IsValid ? StatusCode.OK : StatusCode.BadRequest, this.FormatErrors());
    public Option<T> ToOption<T>() => new Option<T>(IsValid ? StatusCode.OK : StatusCode.BadRequest, this.FormatErrors());
}


public static class ValidationErrorExtensions
{
    public static Option<IValidatorResult> CreateError<T, TProperty>(this IPropertyRule<T, TProperty> propertyRule, string message)
    {
        propertyRule.NotNull();
        message.NotEmpty();

        return new ValidatorError
        {
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

    public static string FormatErrors(this ValidatorResult subject) => subject.NotNull().GetErrors().FormatErrors();

    public static string FormatErrors(this IReadOnlyList<ValidatorError> subject) => subject
        .Select(x => x.ToString())
        .Join(", ");

    public static ValidatorResult ThrowOnError(this ValidatorResult subject) => subject.NotNull()
        .Assert(x => x.IsValid, x => subject.GetErrors().FormatErrors());

    public static ValidatorResult LogResult(this ValidatorResult subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();
        if (subject.IsValid) return subject;

        location.LogError(subject.FormatErrors());
        return subject;
    }
}

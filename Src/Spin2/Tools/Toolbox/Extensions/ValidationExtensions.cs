using FluentValidation;

namespace Toolbox.Extensions;

public static class ValidationExtensions
{
    public static T Verify<T>(this IValidator<T> validator, T subject) => subject.Action(x => validator.ValidateAndThrow(x));
    public static bool IsValid<T>(this IValidator<T> validator, T subject) => validator.Validate(subject).IsValid;

    public static IReadOnlyList<string> GetErrors<T>(this IValidator<T> validator, T subject) => validator
        .Validate(subject)
        .Errors
        .Select(x => x.ErrorMessage)
        .ToArray();
}

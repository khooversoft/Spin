using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;

namespace Toolbox.Types;

public record QueryParameter
{
    public int Index { get; init; } = 0;
    public int Count { get; init; } = 1000;
    public string Domain { get; init; } = null!;
    public string? Filter { get; init; }
    public bool Recursive { get; init; }

    public static QueryParameter Default { get; } = new QueryParameter();
}


public static class QueryParameterValidator
{
    public static Validator<QueryParameter> Validator { get; } = new Validator<QueryParameter>()
        .RuleFor(x => x.Domain).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this QueryParameter subject) => Validator.Validate(subject);
}
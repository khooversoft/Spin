namespace Toolbox.Tools.Validation;

public record ValidatorError : IValidatorResult
{
    public string TypeName { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Message { get; init; } = null!;

    public override string ToString() => $"Type={TypeName}, Nam={Name}, Message={Message}";
}

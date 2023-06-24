namespace Toolbox.Tools.Validation;

public record ValidatorError : IValidateResult
{
    public string Name { get; init; } = null!;
    public string Message { get; init; } = null!;

    public override string ToString() => $"Property {Name}, {Message}";
}

﻿namespace Toolbox.Tools;

public record ValidatorError : IValidatorResult
{
    public string TypeName { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Message { get; init; } = null!;

    public override string ToString() => $"Type={TypeName}, Name={Name}, Message={Message}";
}

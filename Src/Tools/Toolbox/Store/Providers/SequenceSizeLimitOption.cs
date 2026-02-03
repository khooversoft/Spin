using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public record SequenceSizeLimitOption<T>
{
    public string Key { get; set; } = null!;
    public int MaxItems { get; set; } = 1000;
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);

    public static IValidator<SequenceSizeLimitOption<T>> Validator { get; } = new Validator<SequenceSizeLimitOption<T>>()
        .RuleFor(x => x.Key).NotEmpty("Key must be provided")
        .RuleFor(x => x.MaxItems).Must(x => x > 1, x => $"{x} MaxItems must be greater than 1")
        .RuleFor(x => x.CheckInterval).Must(x => x >= TimeSpan.Zero, x => $"{x}, CheckInterval must be greater than zero")
        .Build();
}

public static class SequenceSizeLimitOptionExtensions
{
    public static Option Validate<T>(this SequenceSizeLimitOption<T> option) => SequenceSizeLimitOption<T>.Validator.Validate(option).ToOptionStatus();
}

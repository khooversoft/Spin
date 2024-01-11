using Toolbox.Tools;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record IndexPanel
{
    [Id(0)] public required string Title { get; init; }
    [Id(1)] public required IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();

    public static IValidator<IndexPanel> Validator { get; } = new Validator<IndexPanel>()
        .RuleFor(x => x.Title).NotEmpty()
        .RuleForEach(x => x.Lines).NotEmpty().Must(x => x.Length > 0, _ => $"Must have at least one line")
        .Build();
}

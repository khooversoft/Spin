using Toolbox.Tools;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record IndexGroup
{
    private const string _randomText = "random";

    [Id(0)] public required string GroupName { get; init; }
    [Id(1)] public required string IconName { get; init; } = _randomText;
    [Id(2)] public required string IconColor { get; init; }
    [Id(3)] public int OrderIndex { get; init; } = 1000;

    public bool IsRandom() => IconName == _randomText;

    public static IValidator<IndexGroup> Validator { get; } = new Validator<IndexGroup>()
        .RuleFor(x => x.GroupName).ValidName()
        .RuleFor(x => x.IconName).ValidName()
        .RuleFor(x => x.IconColor).NotEmpty()
        .RuleFor(x => x.OrderIndex).Must(x => x >= 0 && x <= 1000, x => $"Invalid OrderIndex {x}")
        .Build();
}
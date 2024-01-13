using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record OverviewPanel
{
    [Id(0)] public required string Title { get; init; }
    [Id(1)] public required IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
    [Id(2)] public IReadOnlyList<OverviewMenu> Menus { get; init; } = Array.Empty<OverviewMenu>();

    public static IValidator<OverviewPanel> Validator { get; } = new Validator<OverviewPanel>()
        .RuleFor(x => x.Title).NotEmpty()
        .RuleForEach(x => x.Lines).NotEmpty().Must(x => x.Length > 0, _ => $"Must have at least one line")
        .RuleForEach(x => x.Menus).Validate(OverviewMenu.Validator)
        .RuleForObject(x => x).Must(x =>
        {
            var names = x.Menus
                .GroupBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() != 1)
                .Select(x => x.Key)
                .ToArray();

            return names.Length == 0 ? StatusCode.OK : (StatusCode.BadRequest, $"Duplicate title names={names.Join(';')}");
        })
        .Build();
}

[GenerateSerializer, Immutable]
public record OverviewMenu
{
    private const string _randomText = "random";

    [Id(0)] public required string Title { get; init; }
    [Id(1)] public required string IconName { get; init; } = _randomText;
    [Id(2)] public required string IconColor { get; init; }
    [Id(3)] public required string HRef { get; init; }
    [Id(4)] public int OrderIndex { get; init; } = 1000;

    public bool IsRandom() => IconName == _randomText;

    public static IValidator<OverviewMenu> Validator { get; } = new Validator<OverviewMenu>()
        .RuleFor(x => x.Title).NotEmpty()
        .RuleFor(x => x.IconName).ValidName()
        .RuleFor(x => x.IconColor).NotEmpty()
        .RuleFor(x => x.HRef).NotEmpty()
        .RuleFor(x => x.OrderIndex).Must(x => x >= 0 && x <= 1000, x => $"Invalid OrderIndex {x}")
        .Build();
}
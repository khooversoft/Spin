using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphHostOption
{
    public string BasePath { get; set; } = null!;

    public static IValidator<GraphHostOption> Validator { get; } = new Validator<GraphHostOption>()
        .RuleFor(x => x.BasePath).NotEmpty()
        .Build();
}

public static class GraphHostOptionExtensions
{
    public static Option Validate(this GraphHostOption option) => GraphHostOption.Validator.Validate(option).ToOptionStatus();
}

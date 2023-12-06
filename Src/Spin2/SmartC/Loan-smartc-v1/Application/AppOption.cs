using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Loan_smartc_v1.Application;

internal record AppOption
{
    public string ClusterApiUri { get; init; } = null!;
}


internal static class CmdOptionExtensions
{
    public static Validator<AppOption> Validator { get; } = new Validator<AppOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .Build();

    public static AppOption Verify(this AppOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}

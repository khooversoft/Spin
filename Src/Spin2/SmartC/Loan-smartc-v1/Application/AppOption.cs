using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Loan_smartc_v1.Application;

internal record AppOption
{
    public string ClusterApiUri { get; init; } = null!;
    public string AccountId { get; init; } = null!;
    public string BearerToken { get; init; } = null!;
    public string TransactionId { get; init; } = null!;
}


internal static class CmdOptionExtensions
{
    public static Validator<AppOption> Validator { get; } = new Validator<AppOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .RuleFor(x => x.AccountId).ValidAccountId()
        .RuleFor(x => x.BearerToken).NotEmpty()
        .RuleFor(x => x.TransactionId).NotEmpty()
        .Build();

    public static AppOption Verify(this AppOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}

using Toolbox.Tools;

namespace ContractHost.sdk.Model;

public record CreateContractOption
{
    public string Creator { get; init; } = null!;
    public string Description { get; init; } = null!;

    public string SellerUserId { get; init; } = null!;
    public string SellerBankAccountId { get; init; } = null!;
    public string PurchaserUserId { get; init; } = null!;
    public string PurchaserBankAccountId { get; init; } = null!;

    public int NumPayments { get; init; }
    public decimal Principal { get; init; }
    public decimal Payment { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? CompletedDate { get; init; }
}

public static class CreateContractOptionExtensions
{
    public static CreateContractOption Verify(this CreateContractOption option)
    {
        option.NotNull(nameof(option));

        option.Creator.NotNull(nameof(option.Creator));
        option.Description.NotNull(nameof(option.Description));

        option.SellerUserId.NotNull(nameof(option.SellerUserId));
        option.SellerBankAccountId.NotNull(nameof(option.SellerBankAccountId));
        option.PurchaserUserId.NotNull(nameof(option.PurchaserUserId));
        option.PurchaserBankAccountId.NotNull(nameof(option.PurchaserBankAccountId));

        return option;
    }
}

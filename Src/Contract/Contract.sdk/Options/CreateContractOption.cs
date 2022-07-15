//using Toolbox.Tools;

//namespace Contract.sdk.Options;

//public record CreateContractOption
//{
//    public string Creator { get; init; } = null!;
//    public string Description { get; init; } = null!;

//    public string SellerUserId { get; init; } = null!;
//    public string SellerBankAccountId { get; init; } = null!;
//    public string PurchaserUserId { get; init; } = null!;
//    public string PurchaserBankAccountId { get; init; } = null!;

//    public int NumPayments { get; init; }
//    public decimal Principal { get; init; }
//    public decimal Payment { get; init; }
//    public DateTimeOffset StartDate { get; init; }
//    public DateTimeOffset? CompletedDate { get; init; }
//}

//public static class CreateContractOptionExtensions
//{
//    public static CreateContractOption Verify(this CreateContractOption option)
//    {
//        option.NotNull();

//        option.Creator.NotNull();
//        option.Description.NotNull();

//        option.SellerUserId.NotNull();
//        option.SellerBankAccountId.NotNull();
//        option.PurchaserUserId.NotNull();
//        option.PurchaserBankAccountId.NotNull();

//        return option;
//    }
//}

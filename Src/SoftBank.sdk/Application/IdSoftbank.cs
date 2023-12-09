using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public static class IdSoftbank
{
    public const string SoftBankSchema = "softbank";
    public const string SoftBankTrxSchema = "softbank-trx";

    public static bool IsSoftBankId(string? subject) => IdPatterns.IsSchemaDomainMatch(subject, SoftBankSchema);
    public static bool IsSoftBankTrxId(string? subject) => IdPatterns.IsSchemaDomainMatch(subject, SoftBankTrxSchema);

    public static ResourceId CreateSoftBankId(string domain, string accountId) => $"softbank:{domain}/{accountId}";

    public static ResourceId ToSoftBankContractId(this string softbankId) => ResourceId.Create(softbankId)
        .ThrowOnError()
        .Return()
        .ToSoftBankContractId();

    public static ResourceId ToSoftBankContractId(this ResourceId softbankId) => $"contract:{softbankId.Domain}/softbank/{softbankId.Path}";

    public static ResourceId ToSoftBankTrxId(this ResourceId softbankId)
    {
        softbankId.Assert(x => IsSoftBankId(x), x => $"{x} not a valid softbank Id");
        return $"{SoftBankTrxSchema}:{softbankId.Domain}/{softbankId.Path}";
    }
}

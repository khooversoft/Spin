using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public static class IdSoftbank
{
    public static bool IsSoftBankId(string? subject) =>
    subject.IsNotEmpty() &&
    subject.Split(':') switch
    {
        var s when s.Length != 2 => false,
        var s when s[0] != "softbank" => false,
        var s => s.Last().Split('/').Func(x => x.Length > 1 && IdPatterns.IsDomain(x[0]) && x.Skip(1).All(x => IdPatterns.IsPath(x)))
    };

    public static ResourceId CreateSoftBankId(string domain, string accountId) => $"softbank:{domain}/{accountId}";

    public static ResourceId ToSoftBankStorageId(this string softbankId) => ResourceId.Create(softbankId)
        .ThrowOnError()
        .Return()
        .ToSoftBankStorageId();

    public static ResourceId ToSoftBankStorageId(this ResourceId softbankId) => $"contract:{softbankId.Domain}/softbank/{softbankId.Path}";
}

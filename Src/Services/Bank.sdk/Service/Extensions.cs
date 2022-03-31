using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public static class Extensions
{
    public static bool IsValidBankAccount(this DocumentId documentId) => documentId.Path.Split('/').Length >= 2;

    public static string GetBankName(this DocumentId documentId)
    {
        documentId.VerifyNotNull(nameof(documentId));

        return documentId.Path.Split('/').FirstOrDefault() ?? throw new ArgumentNullException($"Bank name does not exist.  Should be 2nd vector");
    }

    public static string GetBankAccount(this DocumentId documentId)
    {
        documentId.VerifyNotNull(nameof(documentId));

        return documentId.Path.Split('/').Skip(1).FirstOrDefault() ?? throw new ArgumentNullException($"Bank account does not exist.  Should be 3rd vector");
    }

    public static bool IsBankName(this DocumentId documentId, string bankName)
    {
        documentId.VerifyNotNull(nameof(documentId));
        bankName.VerifyNotEmpty(nameof(bankName));

        return documentId.GetBankName().Equals(bankName, StringComparison.OrdinalIgnoreCase);
    }
}

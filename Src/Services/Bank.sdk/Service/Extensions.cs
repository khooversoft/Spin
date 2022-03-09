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
    public static bool IsValidBankAccount(this DocumentId documentId) => documentId.Path.Split('/').Length >= 4;

    public static string GetBankName(this DocumentId documentId)
    {
        string[] vectors = documentId.Path.Split('/');
        vectors.VerifyAssert(x => x.Length >= 3, $"BankName is missing in documentId={documentId}");

        return vectors[2];
    }

    public static string GetBankAccount(this DocumentId documentId)
    {
        string[] vectors = documentId.Path.Split('/');
        vectors.VerifyAssert(x => x.Length >= 4, $"Bank Account is missing in documentId={documentId}");

        return vectors[3];
    }
}

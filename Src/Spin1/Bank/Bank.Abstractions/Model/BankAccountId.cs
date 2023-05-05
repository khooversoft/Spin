﻿using Toolbox.Abstractions;

namespace Bank.Abstractions.Model;

public record BankAccountId
{
    public string BankName { get; init; } = null!;

    public string AccountId { get; init; } = null!;

    public override string ToString() => BankName + "/" + AccountId;
}


public static class BankAccountIdExtensions
{
    public static BankAccountId? ToBankAccountId(this DocumentId documentId)
    {
        var vectors = documentId.Vectors;
        if (vectors.Count != 2) return null;

        return new BankAccountId
        {
            BankName = vectors[0],
            AccountId = vectors[1],
        };
    }

    public static string GetBankName(this DocumentId documentId) => documentId.Vectors.First();
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Model;

public record TrxRecord
{
    public DateTime Date { get; init; } = DateTime.UtcNow;

    public TrxType Type { get; init; }

    public decimal Amount { get; init; }

    public IReadOnlyList<string> Properties { get; init; } = null!;
}


public static class TrxRecordExtensions
{
    public static decimal NaturalAmount(this TrxRecord trxRecord) => trxRecord
        .VerifyNotNull(nameof(TrxRequest))
        .Func(x => x.Type switch
        {
            TrxType.Credit => x.Amount,
            TrxType.Debit => -x.Amount,

            _ => throw new ArgumentException($"Unknown type={x.Type}"),
        });
}

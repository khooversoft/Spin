using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;

namespace Bank.Test.Application;

internal class BankTestTool
{
    private readonly DocumentId _firstBankAccountId = (DocumentId)"bank-first/bankAccount1";
    private readonly DocumentId _secondBankAccountId = (DocumentId)"bank-second/bankAccount2";

    public async Task Start()
    {
        BankFirst = new BankTestClient(this, BankName.First);
        BankSecond = new BankTestClient(this, BankName.Second);

        await BankFirst.Start();
        await BankSecond.Start();
    }

    public DocumentId GetBankAcountId(BankName bankName) => bankName switch
    {
        BankName.First => _firstBankAccountId,
        BankName.Second => _secondBankAccountId,

        _ => throw new ArgumentException(),
    };

    public BankTestClient GetPartnerBankClient(BankName bankName) => bankName switch
    {
        BankName.First => BankSecond,
        BankName.Second => BankFirst,

        _ => throw new ArgumentException(),
    };

    public BankTestClient BankFirst { get; set; } = null!;

    public BankTestClient BankSecond { get; set; } = null!;
}


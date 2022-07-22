using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;

namespace Smart_Installment.sdk.Test;

public class InstallmentContractModelTests
{
    [Fact]
    public void DocumentIdEqual_ShouldPass()
    {
        var documentId = (DocumentId)"DocumentId";
        var documentId2 = (DocumentId)"DocumentId";

        (documentId == documentId2).Should().BeTrue();
    }


    [Fact]
    public void HeaderEqual_ShouldPass()
    {
        var header1 = new InstallmentHeader
        {
            PrincipleId = "PrincipleId",
            Name = "Name",
            DocumentId = (DocumentId)"DocumentId",
            Issuer = "Issuer",
            Description = "Description",
            NumPayments = 1,
            Principal = 100.00m,
            Payment = 20.00m,
        };

        var header2 = new InstallmentHeader
        {
            ContractId = header1.ContractId,
            PrincipleId = "PrincipleId",
            Name = "Name",
            DocumentId = (DocumentId)"DocumentId",
            Issuer = "Issuer",
            Description = "Description",
            NumPayments = 1,
            Principal = 100.00m,
            Payment = 20.00m,
        };

        (header1.PrincipleId == header1.PrincipleId).Should().BeTrue();
        (header1.Name == header1.Name).Should().BeTrue();
        (header1.DocumentId == header1.DocumentId).Should().BeTrue();
        (header1.Issuer == header1.Issuer).Should().BeTrue();
        (header1.Description == header1.Description).Should().BeTrue();
        (header1.NumPayments == header1.NumPayments).Should().BeTrue();
        (header1.Principal == header1.Principal).Should().BeTrue();
        (header1.Payment == header1.Payment).Should().BeTrue();


        (header1 == header2).Should().BeTrue();
    }


    [Fact]
    public void Equal_ShouldPass()
    {
        Guid id = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.Now;

        var model1 = new InstallmentContract
        {
            DocumentId = (DocumentId)"test",
            Header = new InstallmentHeader
            {
                PrincipleId = "PrincipleId",
                Name = "Name",
                DocumentId = (DocumentId)"DocumentId",
                Issuer = "Issuer",
                Description = "Description",
                NumPayments = 1,
                Principal = 100.00m,
                Payment = 20.00m,
            },
            CommittedParties = Enumerable.Range(0, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            Parties = Enumerable.Range(10, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            CommittedLedger = Enumerable.Range(0, 2).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
            Ledger = Enumerable.Range(10, 3).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
        };

        var model2 = new InstallmentContract
        {
            DocumentId = (DocumentId)"test",
            Header = new InstallmentHeader
            {
                ContractId = model1.Header.ContractId,
                PrincipleId = "PrincipleId",
                Name = "Name",
                DocumentId = (DocumentId)"DocumentId",
                Issuer = "Issuer",
                Description = "Description",
                NumPayments = 1,
                Principal = 100.00m,
                Payment = 20.00m,
            },
            CommittedParties = Enumerable.Range(0, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            Parties = Enumerable.Range(10, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            CommittedLedger = Enumerable.Range(0, 2).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
            Ledger = Enumerable.Range(10, 3).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
        };

        (model1 == model2).Should().BeTrue();
        (model1 != model2).Should().BeFalse();
    }
    
    [Fact]
    public void NotEqual_ShouldPass()
    {
        Guid id = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.Now;

        var model1 = new InstallmentContract
        {
            DocumentId = (DocumentId)"test",
            Header = new InstallmentHeader
            {
                PrincipleId = "PrincipleId",
                Name = "Name",
                DocumentId = (DocumentId)"DocumentId",
                Issuer = "Issuer",
                Description = "Description",
                NumPayments = 1,
                Principal = 100.00m,
                Payment = 20.00m,
            },
            CommittedParties = Enumerable.Range(0, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            Parties = Enumerable.Range(10, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            CommittedLedger = Enumerable.Range(0, 2).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
            Ledger = Enumerable.Range(10, 3).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
        };

        var model2 = new InstallmentContract
        {
            DocumentId = (DocumentId)"test",
            Header = new InstallmentHeader
            {
                ContractId = model1.Header.ContractId,
                PrincipleId = "PrincipleId",
                Name = "Name",
                DocumentId = (DocumentId)"DocumentId",
                Issuer = "Issuer",
                Description = "Description",
                NumPayments = 1,
                Principal = 100.00m,
                Payment = 20.00m,
            },
            CommittedParties = Enumerable.Range(0, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            Parties = Enumerable.Range(10, 3).Select(x => new PartyRecord
            {
                Id = id,
                Date = now,
                TrxCode = $"add_{x}",
                UserId = $"UserId_{x}",
                PartyType = $"PartyType_{x}",
                BankAccountId = $"BankAccountId_{x}",
            }).ToList(),
            CommittedLedger = Enumerable.Range(0, 12).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
            Ledger = Enumerable.Range(10, 4).Select(x => new LedgerRecord
            {
                Id = id,
                Date = now,
                Type = x % 1 == 0 ? LedgerType.Debit : LedgerType.Credit,
                TrxType = $"Trx_Type_{x}",
                Amount = x * 120.00m,

            }).ToList(),
        };

        (model1 == model2).Should().BeFalse();
    }
}

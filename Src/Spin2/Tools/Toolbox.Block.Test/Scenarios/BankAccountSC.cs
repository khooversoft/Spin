﻿using Microsoft.Extensions.Logging;
using Toolbox.Block.Access;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Test.Scenarios;

public class BankAccountSCActor
{
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<BankAccountSCActor> _logger;

    public BankAccountSCActor(IDocumentStore documentStore, ILogger<BankAccountSCActor> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    public Task<Option<BankAccountSC>> Create(DocumentId documentId, string accountName, string ownerPrincipleId, ScopeContext context)
    {
        return Task.FromResult(new BankAccountSC(documentId, accountName, ownerPrincipleId).ToOption());
    }

    public async Task<Option<BankAccountSC>> Get(DocumentId documentId, ScopeContext context)
    {
        Option<Document> oDocument = await _documentStore.Get(documentId);
        if (oDocument.StatusCode.IsError()) return oDocument.ToOption<BankAccountSC>();

        BlockDocument document = oDocument.Value.ToObject<BlockDocument>();
        var sc = new BankAccountSC(document);

        return new Option<BankAccountSC>(sc);
    }

    public async Task<StatusCode> Set(BankAccountSC sc, ScopeContext context)
    {
        Document document = sc.GetDocument();
        var status = await _documentStore.Set(document, context, document.ETag);
        return status;
    }
}


public class BankAccountSC
{
    private const string _accountText = "AccountMaster";
    private const string _itemLedgerText = "ItemLedger";
    private BlockDocument _document;

    public BankAccountSC(BlockDocument document)
    {
        _document = document;

        AccountMaster account = GetAccountMaster().Assert(x => x.IsSuccess(), "Cannot find account master");
        DocumentId = account.DocumentId;
        AccountName = account.AccountName;
    }

    public BankAccountSC(DocumentId documentId, string accountName, string ownerPrincipleId)
    {
        _document = new BlockDocument(ownerPrincipleId);

        var model = new AccountMaster
        {
            AccountName = accountName,
            DocumentId = documentId,
            OwnerPrincipleId = ownerPrincipleId
        };

        _document.GetScalar(_accountText).Add(model, ownerPrincipleId);
        DocumentId = documentId;
        AccountName = accountName;
    }

    public DocumentId DocumentId { get; }

    public string AccountName { get; }

    public BankAccountSC Add(PrincipalSignature principalSignature) => this.Action(x => _document.Add(principalSignature));

    public BankAccountSC Add(AccountMaster accountMaster, string principleId) => this.Action(x => _document.GetScalar(_accountText).Add(accountMaster, principleId));

    public BankAccountSC AddLedger(string description, LedgerType type, decimal amount, string principleId)
    {
        var ledgerItem = new LedgerItem
        {
            Description = description,
            Type = type,
            Amount = amount
        };

        AddLedger(ledgerItem, principleId);
        return this;
    }

    public string AddLedger(LedgerItem ledgerItem, string principleId)
    {
        string blockId = _document.GetCollection(_itemLedgerText).Add(ledgerItem, principleId);
        return blockId;
    }

    public BankAccountSC Sign() => this.Action(x => _document.Sign());

    public BankAccountSC Validate() => this.Action(x => _document.Validate());

    public Document GetDocument() => _document.GetDocument(DocumentId);

    public Option<AccountMaster> GetAccountMaster() => _document
        .GetScalar(_accountText)
        .Get<AccountMaster>();

    public decimal GetBalance() => _document
        .GetCollection(_itemLedgerText)
        .Get<LedgerItem>()
        .Return()
        .Select(x => x.NaturalAmount).Sum();

    public Option<IReadOnlyList<LedgerItem>> GetLedgerItems() => _document
        .GetCollection(_itemLedgerText)
        .Get<LedgerItem>();
}

public record AccountMaster
{
    public string AccountName { get; init; } = null!;
    public string DocumentId { get; init; } = null!;
    public string OwnerPrincipleId { get; init; } = null!;
    public DateTime? UpdateDate { get; init; } = null!;
    public int Counter { get; init; }
    public string? Message { get; init; } = null!;
}

public enum LedgerType
{
    Credit,
    Debit
}

public static class LedgerTypeExtensions
{
    public static decimal NaturalAmount(this LedgerType type, decimal amount) => type switch
    {
        LedgerType.Credit => Math.Abs(amount),
        LedgerType.Debit => -Math.Abs(amount),

        _ => throw new ArgumentException($"Invalid type={type}")
    };
}

public record LedgerItem
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Description { get; init; } = null!;
    public required LedgerType Type { get; init; }
    public required decimal Amount { get; init; }

    public decimal NaturalAmount => Type.NaturalAmount(Amount);
}
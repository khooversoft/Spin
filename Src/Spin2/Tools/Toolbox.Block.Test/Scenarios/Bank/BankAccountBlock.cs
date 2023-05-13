﻿using Toolbox.Block.Access;
using Toolbox.Block.Test.Scenarios.Bank.Models;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Test.Scenarios.Bank;

public class BankAccountBlock
{
    private const string _accountText = "AccountMaster";
    private const string _itemLedgerText = "ItemLedger";
    private BlockDocument _document;

    public BankAccountBlock(BlockDocument document)
    {
        _document = document;

        AccountMaster account = GetAccountMaster().Assert(x => x.IsSuccess(), "Cannot find account master");
        DocumentId = account.DocumentId;
        AccountName = account.AccountName;
    }

    public BankAccountBlock(DocumentId documentId, string accountName, string ownerPrincipleId)
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

    public BankAccountBlock Add(PrincipalSignature principalSignature) => this.Action(x => _document.Add(principalSignature));

    public BankAccountBlock Add(AccountMaster accountMaster, string principleId) => this.Action(x => _document.GetScalar(_accountText).Add(accountMaster, principleId));

    public BankAccountBlock AddLedger(string description, LedgerType type, decimal amount, string principleId)
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

    public BankAccountBlock Sign() => this.Action(x => _document.Sign());

    public BankAccountBlock Validate() => this.Action(x => _document.Validate());

    public Document GetDocument() => _document.ToDocument(DocumentId);

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

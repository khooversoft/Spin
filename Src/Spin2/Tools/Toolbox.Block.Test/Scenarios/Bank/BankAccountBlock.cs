using Toolbox.Block.Access;
using Toolbox.Block.Test.Scenarios.Bank.Models;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;
using Toolbox.Types.Id;

namespace Toolbox.Block.Test.Scenarios.Bank;

public class BankAccountBlock
{
    private const string _accountText = "AccountMaster";
    private const string _itemLedgerText = "ItemLedger";
    private BlockDocument _document;

    public BankAccountBlock(BlockDocument document)
    {
        _document = document;
    }

    public ObjectId DocumentId => _document.DocumentId;

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

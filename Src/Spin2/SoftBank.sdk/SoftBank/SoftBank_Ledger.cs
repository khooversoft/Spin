using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

internal class SoftBank_Ledger
{
    private readonly SoftBankActor _parent;
    private readonly ILogger _logger;

    public SoftBank_Ledger(SoftBankActor parent, ILogger logger)
    {
        _parent = parent;
        _logger = logger;
    }

    public async Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add Ledger item ledgerItem={ledgerItem}", ledgerItem);

        var v = ledgerItem.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        return await _parent.Append(ledgerItem, ledgerItem.OwnerId, context);
    }

    public async Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting leger items, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        IContractActor contract = _parent.GetContractActor();

        var query = new ContractQuery
        {
            PrincipalId = principalId,
            BlockType = typeof(LedgerItem).GetTypeName(),
        };

        Option<IReadOnlyList<DataBlock>> queryOption = await contract.Query(query, context.TraceId);
        if (queryOption.IsError()) return queryOption.ToOptionStatus<IReadOnlyList<LedgerItem>>();

        IReadOnlyList<LedgerItem> list = queryOption.Return()
            .Select(x => x.ToObject<LedgerItem>())
            .ToArray();

        return list.ToOption();
    }

    public async Task<Option<AccountBalance>> GetBalance(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting leger balance, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        var listOption = await GetLedgerItems(principalId, traceId);
        if (listOption.IsError()) return listOption.ToOptionStatus<AccountBalance>();

        decimal balance = listOption.Return().Sum(x => x.GetNaturalAmount());

        var response = new AccountBalance
        {
            DocumentId = _parent.GetPrimaryKeyString(),
            Balance = balance,
        };

        return response;
    }
}
